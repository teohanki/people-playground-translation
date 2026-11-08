import sys,io,glob
from fontTools.ttLib import TTCollection, TTFont, newTable
from fontTools import subset
from fontTools.pens.cu2quPen import Cu2QuPen
from fontTools.pens.ttGlyphPen import TTGlyphPen
R,ttc,out=sys.argv[1],sys.argv[2],sys.argv[3]
import os
os.makedirs(out,exist_ok=True)
def chars(langs):
    s=set()
    for l in langs:
        for f in glob.glob(f'{R}/translations/{l}/*.txt'):
            for line in open(f,encoding='utf-8'):
                if '\t' in line: s|=set(line.split('\t',1)[1])
    s|=set(open(f'{R}/src/Langs.cs',encoding='utf-8').read())
    s|=set(chr(c) for c in range(0x20,0x7f))
    s|=set(chr(c) for c in range(0x3000,0x3040))
    s|=set(chr(c) for c in range(0xff01,0xff5f))
    s|={'～','…','·','•','°','×','→','←','↑','↓'}
    return s
kana=set(chr(c) for c in range(0x3040,0x3100))
jamo=set(chr(c) for c in range(0x3131,0x318f))
plan={'JP':(0,['ja'],kana),'KR':(1,['ko'],jamo),'SC':(2,['zh-Hans'],set())}
coll=TTCollection(ttc)
for tag,(idx,langs,more) in plan.items():
    buf=io.BytesIO(); coll.fonts[idx].save(buf); buf.seek(0); f=TTFont(buf)
    cs=chars(langs)|more
    opt=subset.Options(); opt.layout_features=['*']; opt.name_IDs=['*']; opt.notdef_outline=True; opt.hinting=False
    sub=subset.Subsetter(opt); sub.populate(text=''.join(cs)); sub.subset(f)
    gs=f.getGlyphSet(); order=f.getGlyphOrder(); glyf={}
    for g in order:
        p=TTGlyphPen(gs); gs[g].draw(Cu2QuPen(p,1.0,reverse_direction=True)); glyf[g]=p.glyph()
    del f['CFF ']
    if 'VORG' in f: del f['VORG']
    t=newTable('glyf'); t.glyphOrder=order; t.glyphs=glyf; f['glyf']=t
    f['loca']=newTable('loca')
    m=newTable('maxp'); m.tableVersion=0x00010000
    for k in ('maxZones','maxTwilightPoints','maxStorage','maxFunctionDefs','maxInstructionDefs','maxStackElements','maxSizeOfInstructions'): setattr(m,k,0)
    m.maxZones=1; m.numGlyphs=len(order); f['maxp']=m
    p=f['post']; p.formatType=2.0; p.extraNames=[]; p.mapping={}; p.glyphOrder=order
    f.sfntVersion='\x00\x01\x00\x00'
    f['head'].indexToLocFormat=0
    fam=f'PPG Noto Sans {tag}'
    nm=f['name']
    for rec in list(nm.names):
        if rec.nameID in (1,3,4,6,16,17): nm.removeNames(nameID=rec.nameID)
    nm.setName(fam,1,3,1,0x409); nm.setName(fam,4,3,1,0x409); nm.setName(fam.replace(' ','')+'-Regular',6,3,1,0x409); nm.setName(fam+' Regular',3,3,1,0x409)
    nm.setName(fam,1,1,0,0); nm.setName(fam,4,1,0,0); nm.setName(fam.replace(' ','')+'-Regular',6,1,0,0)
    fn=f'{out}/NotoSans{tag}-PPG.ttf'; f.save(fn)
    g=TTFont(fn); print(fn, len(g.getGlyphOrder()),'glyphs', len(g.getBestCmap()),'chars')
