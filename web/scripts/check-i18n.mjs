import { existsSync, readFileSync, readdirSync } from 'node:fs'
import { join } from 'node:path'
import { fileURLToPath } from 'node:url'
import { createRequire } from 'node:module'

const require = createRequire(import.meta.url)
const ts = require('typescript')

const root = fileURLToPath(new URL('../src/', import.meta.url))

function flattenKeys(value, prefix = '', keys = []) {
  for (const [key, child] of Object.entries(value)) {
    const path = prefix ? `${prefix}.${key}` : key
    if (child && typeof child === 'object' && !Array.isArray(child)) {
      flattenKeys(child, path, keys)
    } else {
      keys.push(path)
    }
  }
  return keys
}

function readJson(path) {
  return JSON.parse(readFileSync(path, 'utf8'))
}

function findDuplicateJsonKeys(path) {
  const sourceFile = ts.parseJsonText(path, readFileSync(path, 'utf8'))

  function inspect(node, parentKeys = []) {
    if (!ts.isObjectLiteralExpression(node)) {
      ts.forEachChild(node, (child) => inspect(child, parentKeys))
      return
    }

    const seen = new Set()
    for (const property of node.properties) {
      const key = property.name?.text
      if (!key) {
        continue
      }
      const keyPath = [...parentKeys, key]
      if (seen.has(key)) {
        errors.push(`${path} contains duplicate key: ${keyPath.join('.')}`)
      }
      seen.add(key)
      if (ts.isPropertyAssignment(property)) {
        inspect(property.initializer, keyPath)
      }
    }
  }

  inspect(sourceFile)
}

const localePairs = [
  [join(root, 'i18n/locales/en/common.json'), join(root, 'i18n/locales/ja/common.json')],
]

const modulesRoot = join(root, 'modules')
for (const moduleName of readdirSync(modulesRoot)) {
  const english = join(modulesRoot, moduleName, 'i18n/en.json')
  const japanese = join(modulesRoot, moduleName, 'i18n/ja.json')
  if (existsSync(english) || existsSync(japanese)) {
    localePairs.push([english, japanese])
  }
}

const errors = []
const englishKeysByNamespace = new Map()
for (const [englishPath, japanesePath] of localePairs) {
  if (!existsSync(englishPath) || !existsSync(japanesePath)) {
    errors.push(`Missing locale pair: ${englishPath} / ${japanesePath}`)
    continue
  }

  findDuplicateJsonKeys(englishPath)
  findDuplicateJsonKeys(japanesePath)

  const englishKeys = new Set(flattenKeys(readJson(englishPath)))
  const japaneseKeys = new Set(flattenKeys(readJson(japanesePath)))
  const missingJapanese = [...englishKeys].filter((key) => !japaneseKeys.has(key))
  const missingEnglish = [...japaneseKeys].filter((key) => !englishKeys.has(key))
  const namespace = englishPath.includes('/i18n/locales/')
    ? 'common'
    : englishPath.split('/modules/')[1].split('/')[0]
  englishKeysByNamespace.set(namespace, englishKeys)

  if (missingJapanese.length) {
    errors.push(`${japanesePath} is missing: ${missingJapanese.join(', ')}`)
  }
  if (missingEnglish.length) {
    errors.push(`${englishPath} is missing: ${missingEnglish.join(', ')}`)
  }
}

function walkSource(directory, visit) {
  for (const entry of readdirSync(directory, { withFileTypes: true })) {
    const path = join(directory, entry.name)
    if (entry.isDirectory()) {
      walkSource(path, visit)
    } else if (/\.(ts|tsx)$/.test(entry.name)) {
      visit(path)
    }
  }
}

function keyExists(namespace, key) {
  const keys = englishKeysByNamespace.get(namespace)
  return keys?.has(key) || keys?.has(`${key}_one`) || keys?.has(`${key}_other`)
}

walkSource(root, (path) => {
  const source = readFileSync(path, 'utf8')
  const sourceFile = ts.createSourceFile(
    path,
    source,
    ts.ScriptTarget.Latest,
    true,
    path.endsWith('.tsx') ? ts.ScriptKind.TSX : ts.ScriptKind.TS
  )
  let defaultNamespace = 'common'
  let usesTranslation = false

  function findNamespace(node) {
    if (
      ts.isCallExpression(node) &&
      ts.isIdentifier(node.expression) &&
      node.expression.text === 'useTranslation'
    ) {
      usesTranslation = true
      const argument = node.arguments[0]
      if (argument && ts.isStringLiteral(argument)) {
        defaultNamespace = argument.text
      } else if (
        argument &&
        ts.isArrayLiteralExpression(argument) &&
        argument.elements[0] &&
        ts.isStringLiteral(argument.elements[0])
      ) {
        defaultNamespace = argument.elements[0].text
      }
    }
    ts.forEachChild(node, findNamespace)
  }

  findNamespace(sourceFile)
  if (!usesTranslation) {
    return
  }

  function checkTranslationCall(node) {
    if (
      ts.isCallExpression(node) &&
      ts.isIdentifier(node.expression) &&
      node.expression.text === 't' &&
      node.arguments[0] &&
      ts.isStringLiteral(node.arguments[0])
    ) {
      let namespace = defaultNamespace
      let key = node.arguments[0].text
      const namespaceSeparator = key.indexOf(':')
      if (namespaceSeparator >= 0) {
        namespace = key.slice(0, namespaceSeparator)
        key = key.slice(namespaceSeparator + 1)
      }

      const options = node.arguments[1]
      if (options && ts.isObjectLiteralExpression(options)) {
        for (const property of options.properties) {
          if (
            ts.isPropertyAssignment(property) &&
            property.name.getText(sourceFile) === 'ns' &&
            ts.isStringLiteral(property.initializer)
          ) {
            namespace = property.initializer.text
          }
        }
      }

      if (englishKeysByNamespace.has(namespace) && !keyExists(namespace, key)) {
        const { line } = sourceFile.getLineAndCharacterOfPosition(node.getStart(sourceFile))
        errors.push(`${path}:${line + 1} references missing [${namespace}] key: ${key}`)
      }
    }
    ts.forEachChild(node, checkTranslationCall)
  }

  checkTranslationCall(sourceFile)
})

if (errors.length) {
  console.error(errors.join('\n'))
  process.exitCode = 1
} else {
  console.log(
    `i18n locale parity and static key checks passed for ${localePairs.length} namespaces.`
  )
}
