#!/usr/bin/env python3
"""Flag production changes without corresponding test-file changes."""
import subprocess
import sys


def verify_changes(changed, test_changes):
    api = any(p.startswith('src/') and p.endswith(('.cs', '.csproj', '.json')) for p in changed)
    api_tests = any(p.startswith('tests/') and p.endswith('.cs') for p in test_changes)
    return not api or api_tests


def main():
    if len(sys.argv) != 3:
        raise SystemExit('Usage: check_test_changes.py BASE HEAD')
    base, head = sys.argv[1:]
    changed = subprocess.check_output(['git', 'diff', '--name-only', base, head, '--'], text=True).splitlines()
    tests = subprocess.check_output(['git', 'diff', '--name-only', '--diff-filter=ACMR', base, head, '--'], text=True).splitlines()
    if not verify_changes(changed, tests):
        raise SystemExit('API production changes require corresponding application behavior tests; reviewers must assess their coverage.')
    print('Test-change gate passed; reviewers must assess behavioral coverage.')


if __name__ == '__main__':
    main()
