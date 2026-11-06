"""Original synthetic C2 sound beds; stdlib only, deterministic, no external samples."""
from pathlib import Path
import math, random, wave, array, json

RATE=22050
OUT=Path('Assets/_Project/Audio/C2')
OUT.mkdir(parents=True, exist_ok=True)
stats=[]
def write(name, values):
    peak=max(abs(v) for v in values) or 1
    gain=min(1, .7/peak)
    pcm=array.array('h',(int(max(-1,min(1,x*gain))*32767) for x in values))
    with wave.open(str(OUT/(name+'.wav')),'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(RATE); w.writeframes(pcm.tobytes())
    stats.append(dict(name=name,seconds=len(values)/RATE,peak=peak*gain,sampleRate=RATE,source='project-authored synthesis'))
def note(t,f,decay=2):
    return (math.sin(2*math.pi*f*t)+.27*math.sin(2*math.pi*f*2.71*t)+.12*math.sin(2*math.pi*f*4.12*t))*math.exp(-t*decay)*min(1,t*70)
rng=random.Random(26);noise=[rng.uniform(-1,1) for _ in range(RATE*8)]
smooth=0;wind=[]
for i,x in enumerate(noise):
    smooth=.94*smooth+.06*x
    wind.append(smooth*(.38+.09*math.sin(2*math.pi*i/len(noise)))+.015*math.sin(2*math.pi*82*i/RATE))
# Smooth looping join.
for i in range(RATE//4):
    u=i/(RATE//4);wind[-RATE//4+i]=wind[-RATE//4+i]*(1-u)+wind[i]*u
write('Wind_Through_Well',wind)
write('Bell_Bronze',[.55*note(i/RATE,523.25,2.8) for i in range(RATE*2)])
write('ChimLac_Call',[.3*math.sin(2*math.pi*(590*(i/RATE)+80*math.sin(7*(i/RATE))))*math.sin(math.pi*i/(RATE*1.2))**2 for i in range(int(RATE*1.2))])
for combat in [False,True]:
    beat=.28 if combat else .6;length=32*beat;values=[0.]*(int(length*RATE));scale=[220,261.63,293.66,329.63,392]
    for k in range(32):
        if not combat and k%2:continue
        f=scale[[0,2,4,2,1,3,2,0][k%8]]*(1 if combat else 2)
        start=int(k*beat*RATE)
        for j in range(min(int(RATE*2.2),len(values))):
            t=j/RATE;v=.12*note(t,f,2.2 if combat else 1.7)
            if combat and k%2==0:v+=.15*math.sin(2*math.pi*68*t)*math.exp(-t*19)
            values[(start+j)%len(values)]+=v
    write('Music_Arena' if combat else 'Music_Skylight',values)
Path('Logs/C2AudioSynthesis.json').write_text(json.dumps(stats,indent=2),encoding='utf-8')
print(json.dumps(stats))
