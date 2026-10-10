import fs from 'node:fs';
import path from 'node:path';
import {pathToFileURL} from 'node:url';
const source=process.argv[2], output=process.argv[3];
const load=async(p:string)=>import(pathToFileURL(path.join(source,p)).href);
const THREE=await load('node_modules/three/build/three.module.js');
const chars=await load('src/assets/characters.ts');
const homes=await load('src/assets/settlement-homes.ts');
const trees=await load('src/assets/art.ts');
const props=await load('src/assets/props.ts');
const enemies=await load('src/assets/enemies.ts');
const surfaces=await load('src/assets/authored-surfaces.ts');
const {REGIONS}=await load('src/world/regions.ts');
const terrain=await load('src/engine/terrain.ts');
const detail=await load('src/render/environment-detail.ts');
fs.mkdirSync(output,{recursive:true});
const meshes:any[]=[], materials:any[]=[], textures:any[]=[], models:any[]=[];
const meshIds=new Map(),matIds=new Map(),texIds=new Map();
function texture(t:any){
 if(!t||!t.image?.data)return -1;
 if(texIds.has(t.uuid))return texIds.get(t.uuid);
 const id=textures.length;texIds.set(t.uuid,id);
 const data=t.image.data;const file=`texture-${id}.rgba`;
 fs.writeFileSync(path.join(output,file),Buffer.from(data.buffer,data.byteOffset,data.byteLength));
 textures.push({file,width:t.image.width,height:t.image.height,srgb:t.colorSpace==='srgb',repeat:t.repeat.toArray(),offset:t.offset.toArray()});return id;
}
function material(m:any){
 if(matIds.has(m.uuid))return matIds.get(m.uuid);
 const id=materials.length;matIds.set(m.uuid,id);
 materials.push({name:m.name||`surface-${id}`,color:m.color?.toArray()||[1,1,1],emission:m.emissive?.toArray()||[0,0,0],emissionIntensity:m.emissiveIntensity||0,roughness:m.roughness??.8,metallic:m.metalness||0,opacity:m.opacity??1,alphaCutoff:m.alphaTest||0,doubleSided:m.side===2,unlit:m.isMeshBasicMaterial===true,map:texture(m.map),normal:texture(m.normalMap),roughnessMap:texture(m.roughnessMap)});return id;
}
function mesh(g:any){
 if(meshIds.has(g.uuid))return meshIds.get(g.uuid);
 const id=meshes.length;meshIds.set(g.uuid,id);if(!g.attributes.normal)g.computeVertexNormals();
 const p=g.attributes.position,n=g.attributes.normal,uv=g.attributes.uv,c=g.attributes.color;
 const count=p.count, index=g.index;const indices=index?Array.from(index.array):Array.from({length:count},(_,i)=>i);
 const bytes=Buffer.alloc(8+count*12*4+indices.length*4);let off=0;
 bytes.writeInt32LE(count,off);off+=4;
 for(let i=0;i<count;i++)for(const v of [p.getX(i),p.getY(i),-p.getZ(i),n.getX(i),n.getY(i),-n.getZ(i),uv?.getX(i)||0,uv?.getY(i)||0,c?.getX(i)??1,c?.getY(i)??1,c?.getZ(i)??1,1]){bytes.writeFloatLE(v,off);off+=4;}
 bytes.writeInt32LE(indices.length,off);off+=4;
 for(let i=0;i<indices.length;i+=3)for(const v of [indices[i],indices[i+2],indices[i+1]]){bytes.writeInt32LE(v as number,off);off+=4;}
 const file=`mesh-${id}.bin`;fs.writeFileSync(path.join(output,file),bytes);meshes.push({file,vertices:count});return id;
}
function node(o:any):any{
 if(o.isLight)return null;
 if(o.isMesh&&Array.isArray(o.material))throw new Error('Multi-material mesh needs explicit submesh export');
 return {name:o.name||'detail',position:[o.position.x,o.position.y,-o.position.z],rotation:[-o.quaternion.x,-o.quaternion.y,o.quaternion.z,o.quaternion.w],scale:o.scale.toArray(),mesh:o.isMesh?mesh(o.geometry):-1,material:o.isMesh?material(o.material):-1,children:o.children.map(node).filter(Boolean)};
}
function model(name:string,root:any){models.push({name,node:node(root)});}
const nera=chars.createNeraArt();nera.leftLeg.name='leftLeg';nera.rightLeg.name='rightLeg';nera.leftArm.name='nera:arm:left';nera.rightArm.name='nera:arm:right';model('nera',nera.root);
['mara','tavi','imri','sava','oya','bren','sel','rohn','pella','isca','vessa','eiren'].forEach((id,i)=>model(id,chars.createResidentArt({id,name:id,appearance:''},i).group));
for(const region of REGIONS)for(let i=0;i<3;i++)model(`house-${region.id}-${i}`,homes.createSettlementHouseArt(region,i));
for(let i=0;i<6;i++)model(`tree-${i}`,trees.createCanopyTree(i));
model('lantern',trees.createLanternStone());
for(const kind of ['reed','braziers','copper','crate','keg'])model(`prop-${kind}`,props.createSystemicPropArt(kind).group);
for(const [name,role,color] of [['thread-rat','pack',0x786349],['gate-eel','hunter',0x547477],['sentinel','sentinel',0x677784],['brute','brute',0x9b6b43],['wisp','wisp',0x76c2d2]])model(name as string,enemies.createEnemyArt({name,role,color,element:'water',scale:1},name).group);
// The original authored surface maps also cover terrain paths and dungeon masonry.
for(const type of ['cobble','limestone','courtstone','fieldstone']){
 const pixels=surfaces.authoredSurfacePixels(type,256);
 for(const channel of ['albedo','normal','roughness']){
  const file=`${type}-${channel}.rgba`;fs.writeFileSync(path.join(output,file),pixels[channel]);
  textures.push({file,width:256,height:256,srgb:channel==='albedo',repeat:[1,1],offset:[0,0]});
 }
}
for(const [name,build] of [['substrate',detail.createGroundSubstrateTexture],['macro',detail.createGroundMacroTexture],['meadow',detail.createMeadowLeafTexture]]){const t=build();const id=texture(t);textures[id].file=name+'.rgba';fs.writeFileSync(path.join(output,name+'.rgba'),Buffer.from(t.image.data));}
const heights=Buffer.alloc(257*257*4);for(let z=0;z<257;z++)for(let x=0;x<257;x++)heights.writeFloatLE(terrain.heightAt((x/256*250-125)/.17,(z/256*250-125)/.17)*.17,(z*257+x)*4);
fs.mkdirSync(path.join(path.dirname(output),'Assets/Resources'),{recursive:true});fs.writeFileSync(path.join(path.dirname(output),'Assets/Resources/OriginalHeight.bytes'),heights);
fs.writeFileSync(path.join(output,'manifest.json'),JSON.stringify({version:1,sourceCommit:'ac071c727210da99b34d654be92b38311698c58d',meshes,materials,textures,models}));
console.log(JSON.stringify({models:models.length,meshes:meshes.length,materials:materials.length,textures:textures.length,vertices:meshes.reduce((sum,m)=>sum+m.vertices,0)}));
