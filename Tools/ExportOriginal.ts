import { writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';

const original = process.argv[2];
const destination = process.argv[3];
if (!original || !destination) throw new Error('Pass original checkout and output JSON paths.');
const content = await import(pathToFileURL(resolve(original, 'src/content/index.ts')).href);
const catalog = {
  schema: 1,
  sourceCommit: 'ac071c727210da99b34d654be92b38311698c58d',
  regions: content.REGIONS_CONTENT,
  items: content.ITEMS,
  lore: content.LORE,
  npcs: content.NPCS,
  quests: content.QUESTS,
  dungeons: content.DUNGEONS,
};
writeFileSync(destination, JSON.stringify(catalog, null, 2) + '\n');
console.log(JSON.stringify(Object.fromEntries(Object.entries(catalog).filter(([, value]) => Array.isArray(value)).map(([key, value]) => [key, (value as unknown[]).length]))));
