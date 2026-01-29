# ESLint + Prettier Setup Guide

## 🎯 Architecture Overview

This project uses a clear separation of concerns:
- **Prettier** → Code formatting (spacing, line length, semicolons)
- **ESLint** → Code quality & correctness (bugs, patterns, best practices)

## 📦 Installed Tools

- `eslint` + `typescript-eslint` - Linting
- `prettier` - Code formatting
- `husky` - Git hooks
- `lint-staged` - Run linters on staged files only

## ⚙️ Configuration Files

| File | Purpose |
|------|---------|
| `.prettierrc` | Prettier formatting rules (printWidth: 100) |
| `.prettierignore` | Files to skip formatting |
| `eslint.config.js` | ESLint rules and TypeScript config |
| `.lintstagedrc` | Pre-commit hook configuration |
| `.husky/pre-commit` | Git pre-commit hook script |
| `.vscode/settings.json` | VS Code auto-format configuration |
| `.vscode/extensions.json` | Recommended VS Code extensions |

## 🚀 Quick Start

### 1. Install VS Code Extensions
Press `Ctrl+Shift+P` → "Show Recommended Extensions" → Install all

Required:
- ESLint (`dbaeumer.vscode-eslint`)
- Prettier (`esbenp.prettier-vscode`)

### 2. Enable Auto-Format on Save
Already configured in `.vscode/settings.json`
- Format on save: ✅ Enabled
- Fix ESLint on save: ✅ Enabled

### 3. Manual Commands

```bash
# Format all files
npm run format

# Check formatting without changing files
npm run format:check

# Lint and auto-fix
npm run lint:fix

# Type checking
npm run typecheck

# Run all checks (format, lint, types)
npm run validate
```

## 🎨 Prettier Configuration

### Print Width: 100 characters

**Rationale:**
- ✅ Readable on modern displays (1920px+)
- ✅ Reduces excessive line breaks
- ✅ Handles JSX attributes well
- ✅ Good for side-by-side diffs

### Key Settings

```json
{
  "printWidth": 100,          // Max line length
  "semi": false,              // No semicolons (cleaner)
  "singleQuote": true,        // Use single quotes
  "trailingComma": "es5",     // Trailing commas for cleaner diffs
  "tabWidth": 2,              // 2-space indentation
  "arrowParens": "always",    // Always wrap arrow function params
  "bracketSameLine": false    // JSX closing bracket on new line
}
```

### Why No Semicolons?
- Cleaner visual appearance
- ASI (Automatic Semicolon Insertion) handles it
- Industry trend in modern JS/TS

### Handling Long Lines

**JSX Attributes:**
```tsx
// ✅ Good - Prettier wraps automatically
<Component
  prop1="value1"
  prop2="value2"
  prop3="value3"
  onAction={handleAction}
/>
```

**Chained Calls:**
```ts
// ✅ Good - Prettier breaks at 100 chars
const result = data
  .filter((item) => item.active)
  .map((item) => transform(item))
  .slice(0, 10)
```

**Ternaries:**
```ts
// ❌ Avoid nested ternaries (ESLint will error)
const value = cond1 ? 'a' : cond2 ? 'b' : 'c'

// ✅ Use if/else or switch
const getValue = () => {
  if (cond1) return 'a'
  if (cond2) return 'b'
  return 'c'
}
```

## 🔍 ESLint Configuration

### Base Philosophy
- **Errors** → Code that will break or cause bugs
- **Warnings** → Code smells or style issues
- **Off** → Let Prettier or TypeScript handle it

### Key Rules

#### TypeScript Rules (Errors)
```js
'@typescript-eslint/no-explicit-any': 'error'           // Ban 'any' type
'@typescript-eslint/no-unused-vars': 'error'            // Catch unused vars
'@typescript-eslint/consistent-type-imports': 'error'   // Use 'type' imports
'@typescript-eslint/consistent-type-definitions': 'error' // Use 'interface'
```

#### React Rules
```js
'react-hooks/rules-of-hooks': 'error'        // Enforce hooks rules
'react-hooks/exhaustive-deps': 'warn'        // Check dependencies
'react-refresh/only-export-components': 'warn' // HMR optimization
```

#### Code Quality Rules
```js
'no-console': 'warn'              // Warn on console.log (allow .warn/.error)
'no-debugger': 'error'            // Never commit debugger
'no-nested-ternary': 'error'      // Avoid complex ternaries
'eqeqeq': 'error'                 // Always use === 
'curly': 'error'                  // Always use braces
'prefer-const': 'error'           // Use const when possible
```

### Rules to Avoid

❌ **Don't enable these:**
- `max-len` - Let Prettier handle line length
- `indent` - Let Prettier handle indentation
- `quotes` - Let Prettier handle quote style
- `arrow-parens` - Let Prettier handle parens
- Any formatting-related ESLint rules

### Test Files Exception

Test files have relaxed rules:
```js
// **/*.test.{ts,tsx} files allow:
'@typescript-eslint/no-explicit-any': 'off'
'no-console': 'off'
```

## 🔄 Pre-Commit Hooks

### What Happens on `git commit`

1. **Husky** intercepts the commit
2. **lint-staged** runs on staged files only:
   - `prettier --write` → Format files
   - `eslint --fix` → Auto-fix linting issues
3. If all pass → Commit succeeds
4. If any fail → Commit is blocked

### Configuration (`.lintstagedrc`)

```json
{
  "*.{ts,tsx}": [
    "prettier --write",
    "eslint --fix"
  ],
  "*.{json,css}": [
    "prettier --write"
  ]
}
```

### Skipping Hooks (Emergency Only)

```bash
git commit --no-verify -m "Emergency fix"
```

⚠️ Use sparingly! CI will still catch issues.

## 🤖 CI/CD Integration

### GitHub Actions Example

```yaml
name: Lint & Format

on: [push, pull_request]

jobs:
  lint:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: '20'
      
      - run: npm ci
      - run: npm run format:check
      - run: npm run lint
      - run: npm run typecheck
      - run: npm run build
```

### Key Points
- `npm run format:check` → Fail if any file is unformatted
- `npm run lint` → Fail on any ESLint errors
- `npm run typecheck` → Fail on TypeScript errors
- Run in CI even if pre-commit hooks exist

## 👥 Team Best Practices

### 1. Shared Configuration
✅ **DO**: Commit all config files to git
- `.prettierrc`
- `eslint.config.js`
- `.vscode/settings.json`

❌ **DON'T**: Use personal editor configs that override project rules

### 2. Onboarding New Developers

**Checklist for new team members:**
```bash
# 1. Clone repo
git clone <repo-url>
cd web

# 2. Install dependencies
npm install

# 3. Install recommended VS Code extensions
# Press Ctrl+Shift+P → "Show Recommended Extensions"

# 4. Verify setup works
npm run validate

# 5. Make a test commit to verify pre-commit hooks
```

### 3. Handling Legacy Code

When working with unformatted files:
```bash
# Format one file
npm run format -- src/path/to/file.tsx

# Format entire directory
npm run format -- src/modules/analytics/**/*.tsx

# Format everything (use with caution on legacy projects)
npm run format
```

### 4. Resolving Conflicts

**Prettier vs ESLint conflict?**
1. Disable the conflicting ESLint rule
2. Let Prettier handle formatting
3. Keep ESLint for logic/correctness only

**Team disagreement on rules?**
1. Discuss in team meeting
2. Make a decision and document it
3. Update config and commit
4. Everyone runs `npm run format` once

## 🏗️ Monorepo Considerations

This project is part of a monorepo. To share configs:

### Option 1: Shared Config Package
```
.shared/
  eslint-config/
    package.json
    index.js
  prettier-config/
    package.json
    index.js
```

### Option 2: Workspace Inheritance
```json
// Root .prettierrc
{
  "printWidth": 100,
  ...
}

// web/.prettierrc extends root
{
  "extends": "../.prettierrc",
  "overrides": [...]
}
```

## 🚨 Common Mistakes to Avoid

### 1. Double Formatting
❌ **Problem**: Both Prettier and ESLint trying to format
✅ **Solution**: Disable formatting rules in ESLint

### 2. Not Committing Config Files
❌ **Problem**: Each dev has different formatting
✅ **Solution**: Commit `.prettierrc`, `.vscode/settings.json`

### 3. Ignoring Pre-Commit Hooks
❌ **Problem**: Committing unformatted code
✅ **Solution**: Never use `--no-verify` unless emergency

### 4. Too Many Warnings
❌ **Problem**: 100s of warnings → developers ignore them
✅ **Solution**: Either fix them or disable the rule

### 5. Any Type Everywhere
❌ **Problem**: `any` defeats TypeScript purpose
✅ **Solution**: ESLint rule `no-explicit-any: 'error'`

### 6. Formatting node_modules
❌ **Problem**: Prettier trying to format dependencies
✅ **Solution**: Use `.prettierignore`

### 7. Inconsistent Line Endings
❌ **Problem**: Git showing changes on every line
✅ **Solution**: Set `endOfLine: "lf"` in Prettier

## 🎯 Best Practice Rules of Thumb

1. **Prettier owns formatting** → Never add formatting rules to ESLint
2. **ESLint owns correctness** → Focus on bugs and patterns
3. **Auto-fix everything possible** → Minimize manual work
4. **Block commits on errors** → Prevent broken code from entering repo
5. **Run in CI** → Catch issues that bypass pre-commit hooks
6. **Team consensus** → Don't change configs without team agreement
7. **Document exceptions** → If you disable a rule, comment why

## 📊 Rule Categories

### 🔴 Errors (Block Commits)
- Type safety violations
- React hooks violations
- Security issues (eval, etc.)
- Logic errors (== instead of ===)

### 🟡 Warnings (Allow but Flag)
- Missing dependencies in useEffect
- console.log statements
- Non-null assertions

### ⚪ Off (Not Checked)
- Formatting rules (Prettier handles)
- Stylistic preferences (team choice)
- Overly strict rules (prefer-arrow-callback)

## 🔧 Troubleshooting

### ESLint not working in VS Code?
1. Check `.vscode/settings.json` exists
2. Verify ESLint extension is installed
3. Restart VS Code
4. Check output: View → Output → ESLint

### Prettier not formatting on save?
1. Check default formatter: Right-click file → Format Document With → Prettier
2. Verify `.prettierrc` exists
3. Check `.prettierignore` isn't excluding your file

### Pre-commit hooks not running?
1. Check `.husky/pre-commit` file exists
2. Verify file has execute permissions (Unix/Mac)
3. Check `prepare` script ran: `npm run prepare`

### Lint errors on valid code?
1. Check if ESLint config is outdated
2. Update dependencies: `npm update`
3. Clear cache: `rm -rf node_modules .eslintcache && npm install`

## 📚 Additional Resources

- [Prettier Options](https://prettier.io/docs/en/options.html)
- [ESLint Rules](https://eslint.org/docs/rules/)
- [TypeScript ESLint Rules](https://typescript-eslint.io/rules/)
- [React Hooks Rules](https://react.dev/reference/rules/rules-of-hooks)

## 🎓 Project-Specific Rules

Based on `.github/copilot-instructions.md`:

### No Comments Policy
- ❌ NO XML documentation comments
- ❌ NO single-line comments (//)
- ❌ NO multi-line comments (/* */)
- ✅ Self-documenting code with clear naming

### Module Organization
```
module-name/
├── api/          # API calls
├── components/   # React components  
├── hooks/        # Custom hooks
├── pages/        # Page components
├── types/        # Type definitions
└── utils/        # Utility functions
```

### Conditional Logic
```ts
// ❌ Chained ternaries
const x = a ? b : c ? d : e

// ✅ Switch statements
switch (true) {
  case a: return b
  case c: return d
  default: return e
}

// ✅ Early returns
if (a) return b
if (c) return d
return e
```

---

**Last Updated:** January 29, 2026  
**Maintainer:** Development Team  
**Questions?** Open an issue or ask in team chat
