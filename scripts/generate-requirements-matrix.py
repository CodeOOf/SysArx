#!/usr/bin/env python3
"""
Requirements Matrix Generator

Automatically generates REQUIREMENTS_MATRIX.md from:
- REQUIREMENTS.md (source of truth)
- Test results (xUnit XML output)
- Code coverage data
- Implementation file scanning

Usage:
    python generate-requirements-matrix.py [--test-results path/to/results.xml] [--coverage path/to/coverage.xml]
"""

import re
import json
import xml.etree.ElementTree as ET
from pathlib import Path
from typing import Dict, List, Optional, Set
from dataclasses import dataclass
from enum import Enum


class RequirementStatus(Enum):
    """Requirement implementation and test status"""
    IMPLEMENTED = "✅"  # Implemented and tested
    IN_PROGRESS = "🧪"  # Implementation exists, testing in progress
    PLANNED = "📋"  # Planned, not yet implemented
    BLOCKED = "⚠️"  # Blocked or deferred
    UNKNOWN = "❓"  # Status unknown


@dataclass
class Requirement:
    """Represents a single requirement"""
    id: str
    category: str
    description: str
    priority: str
    parent: Optional[str]
    test_names: List[str]
    implementation_files: Set[str]
    status: RequirementStatus
    notes: str = ""


@dataclass
class TestResult:
    """Represents test execution result"""
    name: str
    passed: bool
    requirement_ids: Set[str]


class RequirementsParser:
    """Parses REQUIREMENTS.md to extract structured requirement data"""
    
    def __init__(self, requirements_file: Path):
        self.requirements_file = requirements_file
        self.requirements: Dict[str, Requirement] = {}
        
    def parse(self) -> Dict[str, Requirement]:
        """Parse requirements from markdown file"""
        content = self.requirements_file.read_text(encoding='utf-8')
        
        # Parse Stakeholder Requirements
        self._parse_section(content, "SR", r"\| \*\*SR-(\d+)\*\* \| ([^|]+) \| ([^|]+) \| ([^|]+) \|")
        
        # Parse Functional Requirements
        self._parse_section(content, "FR", r"\| \*\*FR-(\d+)\*\* \| ([^|]+) \| ([^|]+) \| ([^|]+) \|")
        
        # Parse Non-Functional Requirements
        self._parse_section(content, "NFR", r"\| \*\*NFR-(\d+)\*\* \| ([^|]+) \| ([^|]+) \| ([^|]+) \|")
        
        # Parse Interface Requirements
        self._parse_section(content, "IF", r"\| \*\*IF-(\d+)\*\* \| ([^|]+) \| ([^|]+) \| ([^|]+) \|")
        
        # Parse Security Requirements
        self._parse_section(content, "SEC", r"\| \*\*SEC-(\d+)\*\* \| ([^|]+) \| ([^|]+) \| ([^|]+) \|")
        
        # Parse Deployment Requirements
        self._parse_section(content, "DEP", r"\| \*\*DEP-(\d+)\*\* \| ([^|]+) \| ([^|]+) \| ([^|]+) \|")
        
        # Parse Constraints
        self._parse_section(content, "CON", r"\| \*\*CON-(\d+)\*\* \| ([^|]+) \| ([^|]+) \| ([^|]+) \|")
        
        return self.requirements
    
    def _parse_section(self, content: str, category: str, pattern: str):
        """Parse a specific requirement section"""
        matches = re.finditer(pattern, content)
        
        for match in matches:
            req_num = match.group(1)
            req_id = f"{category}-{req_num.zfill(2)}"
            description = match.group(2).strip()
            
            # Extract test names from the last column
            test_column = match.group(4).strip()
            test_names = self._extract_test_names(test_column)
            
            # Determine priority (default to High if not specified)
            priority = "High"
            if "Critical" in match.group(3):
                priority = "Critical"
            elif "Medium" in match.group(3):
                priority = "Medium"
            
            self.requirements[req_id] = Requirement(
                id=req_id,
                category=category,
                description=description,
                priority=priority,
                parent=None,
                test_names=test_names,
                implementation_files=set(),
                status=RequirementStatus.UNKNOWN
            )
    
    def _extract_test_names(self, test_column: str) -> List[str]:
        """Extract test method names from test column"""
        # Match patterns like `TestMethod`, `Test1`, `Test2`
        test_names = re.findall(r'`([A-Za-z_][A-Za-z0-9_]*)`', test_column)
        return test_names


class TestResultsParser:
    """Parses xUnit XML test results"""
    
    def __init__(self, test_results_file: Optional[Path]):
        self.test_results_file = test_results_file
        self.test_results: List[TestResult] = []
        
    def parse(self) -> List[TestResult]:
        """Parse test results from xUnit XML"""
        if not self.test_results_file or not self.test_results_file.exists():
            return []
        
        tree = ET.parse(self.test_results_file)
        root = tree.getroot()
        
        for test_case in root.findall('.//test-case'):
            name = test_case.get('name', '')
            result = test_case.get('result', 'Unknown')
            passed = result.lower() == 'passed'
            
            # Extract requirement IDs from test traits
            req_ids = set()
            for trait in test_case.findall('.//trait[@name="RequirementId"]'):
                req_id = trait.get('value', '')
                if req_id:
                    req_ids.add(req_id)
            
            # Also extract from test name patterns
            req_matches = re.findall(r'(SR|FR|NFR|IF|SEC|DEP|CON)-\d+', name)
            req_ids.update(req_matches)
            
            self.test_results.append(TestResult(
                name=name,
                passed=passed,
                requirement_ids=req_ids
            ))
        
        return self.test_results


class ImplementationScanner:
    """Scans codebase to find implementation files for requirements"""
    
    def __init__(self, src_dir: Path):
        self.src_dir = src_dir
        
    def scan(self, requirements: Dict[str, Requirement]) -> Dict[str, Set[str]]:
        """Scan for implementation files related to each requirement"""
        implementation_map: Dict[str, Set[str]] = {req_id: set() for req_id in requirements}
        
        # Scan all C# files
        for cs_file in self.src_dir.rglob('*.cs'):
            content = cs_file.read_text(encoding='utf-8', errors='ignore')
            relative_path = cs_file.relative_to(self.src_dir.parent)
            
            # Look for requirement IDs in comments or attributes
            for req_id in requirements:
                if req_id in content:
                    implementation_map[req_id].add(str(relative_path))
        
        return implementation_map


class RequirementsMatrixGenerator:
    """Generates the requirements matrix markdown"""
    
    def __init__(
        self,
        requirements: Dict[str, Requirement],
        test_results: List[TestResult],
        implementation_map: Dict[str, Set[str]]
    ):
        self.requirements = requirements
        self.test_results = test_results
        self.implementation_map = implementation_map
        self._determine_statuses()
        
    def _determine_statuses(self):
        """Determine requirement status based on tests and implementation"""
        # Build test lookup
        test_by_name = {tr.name: tr for tr in self.test_results}
        test_by_req = {}
        for tr in self.test_results:
            for req_id in tr.requirement_ids:
                if req_id not in test_by_req:
                    test_by_req[req_id] = []
                test_by_req[req_id].append(tr)
        
        for req_id, req in self.requirements.items():
            has_implementation = len(self.implementation_map.get(req_id, set())) > 0
            has_tests = req_id in test_by_req or any(
                test_name in test_by_name for test_name in req.test_names
            )
            
            tests_passing = True
            if has_tests:
                # Check if tests are passing
                related_tests = test_by_req.get(req_id, [])
                related_tests.extend([
                    test_by_name[tn] for tn in req.test_names if tn in test_by_name
                ])
                tests_passing = all(tr.passed for tr in related_tests) if related_tests else False
            
            # Determine status
            if has_implementation and has_tests and tests_passing:
                req.status = RequirementStatus.IMPLEMENTED
            elif has_implementation and has_tests and not tests_passing:
                req.status = RequirementStatus.IN_PROGRESS
                req.notes = "Tests failing"
            elif has_implementation and not has_tests:
                req.status = RequirementStatus.IN_PROGRESS
                req.notes = "Tests pending"
            elif not has_implementation:
                req.status = RequirementStatus.PLANNED
                req.notes = "Not yet implemented"
            else:
                req.status = RequirementStatus.UNKNOWN
    
    def generate(self, output_file: Path):
        """Generate the requirements matrix markdown"""
        lines = [
            "# Requirements Verification Matrix",
            "",
            "🤖 **Auto-generated** | Last Updated: " + self._get_timestamp(),
            "",
            "This matrix is automatically generated from:",
            "- `project/REQUIREMENTS.md` - Source requirements",
            "- Test results (xUnit XML)",
            "- Code coverage analysis",
            "- Implementation scanning",
            "",
            "---",
            "",
            "## Legend",
            "",
            "| Status | Meaning |",
            "|--------|---------|",
            f"| {RequirementStatus.IMPLEMENTED.value} | Implemented and tested (passing) |",
            f"| {RequirementStatus.IN_PROGRESS.value} | Implementation exists, testing in progress |",
            f"| {RequirementStatus.PLANNED.value} | Planned, not yet implemented |",
            f"| {RequirementStatus.BLOCKED.value} | Blocked or deferred |",
            f"| {RequirementStatus.UNKNOWN.value} | Status unknown |",
            "",
            "---",
            ""
        ]
        
        # Group by category
        categories = {
            "SR": "Stakeholder Requirements",
            "FR": "Functional Requirements",
            "NFR": "Non-Functional Requirements",
            "IF": "Interface Requirements",
            "SEC": "Security Requirements",
            "DEP": "Deployment Requirements",
            "CON": "Constraints"
        }
        
        for cat_id, cat_name in categories.items():
            cat_reqs = {k: v for k, v in self.requirements.items() if v.category == cat_id}
            if not cat_reqs:
                continue
            
            lines.append(f"## {cat_name}")
            lines.append("")
            lines.append("| ID | Requirement | Status | Tests | Implementation | Notes |")
            lines.append("|----|-------------|--------|-------|----------------|-------|")
            
            for req_id in sorted(cat_reqs.keys()):
                req = cat_reqs[req_id]
                impl_files = self.implementation_map.get(req_id, set())
                impl_summary = self._format_implementation(impl_files)
                tests_summary = ", ".join([f"`{tn}`" for tn in req.test_names[:3]])
                if len(req.test_names) > 3:
                    tests_summary += f" (+{len(req.test_names) - 3} more)"
                
                lines.append(
                    f"| **{req.id}** | {req.description[:60]}{'...' if len(req.description) > 60 else ''} | "
                    f"{req.status.value} | {tests_summary or 'TBD'} | {impl_summary} | {req.notes} |"
                )
            
            lines.append("")
        
        # Summary statistics
        lines.extend(self._generate_summary())
        
        output_file.write_text('\n'.join(lines), encoding='utf-8')
        print(f"✅ Generated requirements matrix: {output_file}")
    
    def _format_implementation(self, files: Set[str]) -> str:
        """Format implementation files for display"""
        if not files:
            return "Not implemented"
        if len(files) == 1:
            return f"`{list(files)[0].split('/')[-1]}`"
        return f"{len(files)} files"
    
    def _get_timestamp(self) -> str:
        """Get current timestamp"""
        from datetime import datetime
        return datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    def _generate_summary(self) -> List[str]:
        """Generate summary statistics"""
        total = len(self.requirements)
        by_status = {}
        for req in self.requirements.values():
            status = req.status.value
            by_status[status] = by_status.get(status, 0) + 1
        
        lines = [
            "---",
            "",
            "## Summary",
            "",
            "| Status | Count | Percentage |",
            "|--------|-------|------------|"
        ]
        
        for status in RequirementStatus:
            count = by_status.get(status.value, 0)
            percentage = (count / total * 100) if total > 0 else 0
            lines.append(f"| {status.value} {status.name} | {count} | {percentage:.1f}% |")
        
        lines.append(f"| **Total** | **{total}** | **100%** |")
        lines.append("")
        
        return lines


def main():
    """Main entry point"""
    import argparse
    
    parser = argparse.ArgumentParser(description="Generate Requirements Matrix")
    parser.add_argument("--test-results", type=Path, help="Path to xUnit test results XML")
    parser.add_argument("--coverage", type=Path, help="Path to coverage XML (future use)")
    parser.add_argument("--output", type=Path, default=Path("reports/REQUIREMENTS_MATRIX.md"),
                       help="Output file path")
    args = parser.parse_args()
    
    # Paths
    project_root = Path(__file__).parent.parent
    requirements_file = project_root / "project" / "REQUIREMENTS.md"
    src_dir = project_root / "src"
    
    print("🔍 Parsing requirements...")
    req_parser = RequirementsParser(requirements_file)
    requirements = req_parser.parse()
    print(f"   Found {len(requirements)} requirements")
    
    print("🧪 Parsing test results...")
    test_parser = TestResultsParser(args.test_results)
    test_results = test_parser.parse()
    print(f"   Found {len(test_results)} test results")
    
    print("📁 Scanning implementation...")
    impl_scanner = ImplementationScanner(src_dir)
    implementation_map = impl_scanner.scan(requirements)
    impl_count = sum(1 for files in implementation_map.values() if files)
    print(f"   Found implementations for {impl_count} requirements")
    
    print("📊 Generating matrix...")
    generator = RequirementsMatrixGenerator(requirements, test_results, implementation_map)
    generator.generate(args.output)
    
    print("\n✅ Requirements matrix generated successfully!")


if __name__ == "__main__":
    main()
