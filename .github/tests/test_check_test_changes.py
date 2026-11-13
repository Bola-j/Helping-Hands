import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location('gate', Path(__file__).parents[1] / 'scripts' / 'check_test_changes.py')
gate = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gate)


class TestChangeGate(unittest.TestCase):
    def test_api_change_without_tests_is_rejected(self):
        self.assertFalse(gate.verify_changes(['src/HelpingHands.Api/Program.cs'], []))
        self.assertFalse(gate.verify_changes(['src/HelpingHands.Api/Program.cs'], ['.github/tests/gate.py']))

    def test_api_change_with_test_is_accepted(self):
        self.assertTrue(gate.verify_changes(['src/HelpingHands.Api/Program.cs'], ['tests/Api.Tests/Behavior.cs']))

    def test_workflow_and_infrastructure_changes_do_not_claim_application_tests(self):
        self.assertTrue(gate.verify_changes(['.github/workflows/api-ci.yml'], []))
        self.assertTrue(gate.verify_changes(['compose.yaml'], []))

    def test_docs_only_do_not_require_tests(self):
        self.assertTrue(gate.verify_changes(['docs/example.md'], []))


if __name__ == '__main__':
    unittest.main()
