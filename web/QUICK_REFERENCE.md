# 🚀 Quick Reference: ESLint + Prettier

## 📋 Daily Commands

```bash
# Format all files
npm run format

# Check if files are formatted
npm run format:check

# Lint and auto-fix
npm run lint:fix

# Run all checks
npm run validate
```

## 🎨 Prettier Rules (printWidth: 100)

| Rule | Value | Why |
|------|-------|-----|
| `printWidth` | 100 | Modern displays, readable code |
| `semi` | false | Cleaner look, ASI handles it |
| `singleQuote` | true | Less visual noise |
| `trailingComma` | es5 | Cleaner git diffs |
| `tabWidth` | 2 | Standard for JS/TS |

## 🔍 ESLint Key Rules

### ❌ Errors (Block Commits)
- `@typescript-eslint/no-explicit-any` - Ban `any` type
- `@typescript-eslint/no-unused-vars` - Catch unused variables
- `react-hooks/rules-of-hooks` - Enforce hooks rules
- `no-nested-ternary` - Avoid complex ternaries
- `eqeqeq` - Always use `===`
- `no-debugger` - Never commit debugger

### ⚠️ Warnings (Flag but Allow)
- `react-hooks/exhaustive-deps` - Check useEffect deps
- `no-console` - Warn on console.log (allow .warn/.error)
- `@typescript-eslint/no-non-null-assertion` - Avoid `!` operator

## 🔄 Pre-Commit Workflow

```
git add .
git commit -m "message"
  ↓
Husky intercepts
  ↓
lint-staged runs on staged files:
  1. prettier --write
  2. eslint --fix
  ↓
✅ Pass → Commit succeeds
❌ Fail → Commit blocked
```

## 🛠️ VS Code Setup

### Required Extensions
- ESLint (`dbaeumer.vscode-eslint`)
- Prettier (`esbenp.prettier-vscode`)

### Auto-Format on Save
Already configured in `.vscode/settings.json`:
- ✅ Format on save
- ✅ Fix ESLint on save
- ✅ Organize imports disabled (ESLint handles it)

## 🚨 Common Fixes

### ESLint not working?
1. Restart VS Code
2. Check ESLint extension installed
3. View → Output → ESLint

### Prettier not formatting?
1. Right-click → Format Document With → Prettier
2. Check `.prettierrc` exists
3. Check file not in `.prettierignore`

### Pre-commit not running?
```bash
cd web
npm run prepare
```

## 📏 Code Patterns

### ❌ Avoid
```ts
// Nested ternaries
const x = a ? b : c ? d : e

// Using 'any'
const data: any = {}

// console.log
console.log('debug')

// == instead of ===
if (x == y) {}
```

### ✅ Prefer
```ts
// Switch or early returns
if (a) return b
if (c) return d
return e

// Proper types
const data: UserData = {}

// console.warn/error only
console.error('Error:', err)

// Always ===
if (x === y) {}
```

## 🔧 Utilities & Types Organization

```
module/
├── api/          # API calls
├── components/   # React components (props here)
├── hooks/        # Custom hooks
├── types/        # Shared types/interfaces
└── utils/        # Utility functions
```

**Rules:**
- ✅ Utility functions → `utils/` folder
- ✅ Shared types → `types/` folder
- ✅ Component props → Keep in component file
- ❌ Never define utils inside components
- ❌ Never define shared types inside components

## 🎯 Emergency Commands

```bash
# Skip pre-commit (use sparingly!)
git commit --no-verify -m "Emergency fix"

# Format single file
npm run format -- src/path/to/file.tsx

# Fix all ESLint errors
npm run lint:fix
```

## 📊 What Gets Checked Where

| Check | Pre-Commit | CI | Editor |
|-------|-----------|-----|--------|
| Prettier format | ✅ Auto-fix | ✅ Fail if wrong | ✅ On save |
| ESLint errors | ✅ Auto-fix | ✅ Fail | ✅ Show inline |
| TypeScript types | ❌ | ✅ Fail | ✅ Show inline |
| Build | ❌ | ✅ Fail | ❌ |

## 🏆 Best Practices

1. **Never use `--no-verify`** unless emergency
2. **Commit config files** to ensure team consistency
3. **Run `npm run validate`** before pushing
4. **Let Prettier handle formatting** - don't fight it
5. **Fix warnings eventually** - they become noise
6. **Use explicit types** - avoid `any`
7. **Self-documenting code** - no comments needed

## 📚 Files Reference

| File | Purpose |
|------|---------|
| `.prettierrc` | Formatting rules |
| `eslint.config.js` | Linting rules |
| `.lintstagedrc` | Pre-commit config |
| `.husky/pre-commit` | Git hook script |
| `.vscode/settings.json` | Editor config |
| `.github/workflows/frontend-lint.yml` | CI pipeline |

---

**Need help?** Read [ESLINT_PRETTIER_SETUP.md](./ESLINT_PRETTIER_SETUP.md) for detailed guide.
