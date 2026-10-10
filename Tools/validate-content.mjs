import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';

const catalog = JSON.parse(readFileSync(new URL('../Assets/Resources/game-content.json', import.meta.url), 'utf8'));
const linker = readFileSync(new URL('../Assets/link.xml', import.meta.url), 'utf8');
for (const collider of ['BoxCollider', 'SphereCollider', 'CapsuleCollider']) assert.ok(linker.includes(`fullname="UnityEngine.${collider}" preserve="all"`), `${collider} preserved for runtime primitive creation`);
assert.equal(catalog.schema, 1);
assert.equal(catalog.sourceCommit, 'ac071c727210da99b34d654be92b38311698c58d');
const identifiers = {};
for (const collection of ['regions', 'items', 'lore', 'npcs', 'quests', 'dungeons']) {
  assert.ok(Array.isArray(catalog[collection]) && catalog[collection].length > 0, `${collection} is populated`);
  identifiers[collection] = new Set(catalog[collection].map(item => item.id));
  assert.equal(identifiers[collection].size, catalog[collection].length, `${collection} IDs are unique`);
}
assert.equal(catalog.regions.length, 8);
assert.equal(catalog.dungeons.length, 7);
assert.equal(catalog.siteMechanisms.length, 24);
const sites = catalog.regions.flatMap(region => region.sites);
assert.equal(sites.length, 24);
assert.deepEqual(new Set(catalog.siteMechanisms.map(site => site.id)), new Set(sites.map(site => site.id)), 'every outdoor site has its authored mechanism');
for (const collection of ['npcs', 'lore', 'dungeons']) {
  for (const item of catalog[collection]) assert.ok(identifiers.regions.has(item.region), `${item.id}: valid region`);
}
for (const dungeon of catalog.dungeons) {
  const rooms = new Set(dungeon.rooms.map(room => room.id));
  assert.equal(rooms.size, dungeon.rooms.length, `${dungeon.id}: unique rooms`);
  for (const link of dungeon.links) {
    assert.ok(rooms.has(link.from) && rooms.has(link.to), `${dungeon.id}: connected link endpoints exist`);
  }
  for (const puzzle of dungeon.puzzles) {
    assert.ok(rooms.has(puzzle.roomId), `${puzzle.id}: puzzle room exists`);
    for (const step of puzzle.steps) if (step.roomId) assert.ok(rooms.has(step.roomId), `${puzzle.id}: station room exists`);
  }
  assert.ok(rooms.has(dungeon.boss.roomId), `${dungeon.id}: boss room exists`);
}
console.log(JSON.stringify({status: 'passed', collections: Object.fromEntries(Object.keys(identifiers).map(key => [key, catalog[key].length]))}));
