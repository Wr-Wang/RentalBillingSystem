const fs = require('fs');
const path = require('path');

function walk(dir) {
  let files = [];
  try {
    const entries = fs.readdirSync(dir, { withFileTypes: true });
    for (const f of entries) {
      const p = dir + '/' + f.name;
      if (f.isDirectory()) files = files.concat(walk(p));
      else if (f.name.endsWith('.vue')) files.push(p);
    }
  } catch (e) {}
  return files;
}

const vueFiles = walk('src/views');
let errors = [];

for (const file of vueFiles) {
  const content = fs.readFileSync(file, 'utf8');
  const m = content.match(/<script setup>([\s\S]*?)<\/script>/);
  if (!m) continue;
  const s = m[1];
  const vi = s.match(/import\s+\{([^}]+)\}\s+from\s+'vue'/);
  if (!vi) continue;
  const imported = vi[1].split(',').map(x => x.trim().split(' as ')[0].trim());

  if (s.includes('reactive(') && !imported.includes('reactive'))
    errors.push(file + ': missing reactive');
  if (s.includes('ref(') && !imported.includes('ref'))
    errors.push(file + ': missing ref');
  if (s.includes('computed(') && !imported.includes('computed'))
    errors.push(file + ': missing computed');
}

if (errors.length === 0) {
  console.log('All import checks passed');
} else {
  errors.forEach(e => console.log('ERROR: ' + e));
}
