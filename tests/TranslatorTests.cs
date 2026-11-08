using System; using PPGTranslate;
class T{ static int fail=0;
 static void Eq(Translator t,string i,string e){ var r=t.Translate(i); bool ok=r==e; if(!ok)fail++; Console.WriteLine((ok?"OK   ":"FAIL ")+Show(i)+" => "+Show(r)+(ok?"":"   (ждали "+Show(e)+")"));}
 static string Show(string s){return s==null?"null":"\""+s.Replace("\n","\\n")+"\"";}
 static void Main(string[] a){ var t=new Translator(); foreach(var sub in new[]{"_all","ru"}){ var fs=System.IO.Directory.GetFiles(System.IO.Path.Combine(a[0],sub),"*.txt"); Array.Sort(fs,StringComparer.OrdinalIgnoreCase); foreach(var f in fs) Console.WriteLine(f+": "+t.LoadFile(f)); }
  t.DumpEnabled=true; t.DumpPath=a[1]; if(System.IO.File.Exists(a[1]))System.IO.File.Delete(a[1]);
  Eq(t,"Copy","Копировать"); Eq(t,"  Copy ","  Копировать "); Eq(t,"copy","копировать"); Eq(t,"COPY","КОПИРОВАТЬ");
  Eq(t,"play","играть"); Eq(t,"PLAY","ИГРАТЬ"); Eq(t,"Play","Играть");
  Eq(t,"Set hover height.","Задать высоту парения."); Eq(t,"Set hover height in meters:","Задать высоту парения в метрах:");
  Eq(t,"<b>Copy</b>","<b>Копировать</b>"); Eq(t,"<size=65%>Tools","<size=65%><size=60%>Инстр.");
  Eq(t,"Knife\nA sharp metal sword.","Нож\nОстрый металлический меч."); Eq(t,"Knife\r\nUnknown thing","Нож\r\nUnknown thing");
  Eq(t,"FPS: 60","FPS: 60"); Eq(t,"","" ); Eq(t,null,null); Eq(t,"Привет","Привет"); Eq(t,"12345","12345");
  Eq(t,"Totally unknown string here","Totally unknown string here"); Eq(t,"Totally unknown string here","Totally unknown string here");
  Eq(t,"Set tank to push mode.","Перевести бак в режим выталкивания.");
  Eq(t,"<color=#F55><b>It is recommended to keep this enabled. Mods that ask you to disable this setting should be reported.","<color=#F55><b>Рекомендуется держать включённым. На моды, которые просят это отключить, стоит жаловаться.");
  Eq(t,"Reject suspicious mods","Отклонять подозрительные моды"); Eq(t,"<","<"); Eq(t,"<<>>","<<>>"); Eq(t,"a<b","a<b"); Eq(t,"\n\n","\n\n"); Eq(t," . ", " . "); Eq(t,".",".");
  Eq(t,"<color=#44FF44>Green signals<color=#FFFFFF> toggle. <color=#FF4444>Red signals<color=#FFFFFF> open. <color=#7777FF>Blue signals<color=#FFFFFF> close.","Зелёные сигналы переключают. Красные открывают. Синие закрывают.");
  Eq(t,"<b>Copy</b> something unknown","<b>Copy</b> something unknown");
  Eq(t,"komnata1","komnata1"); Eq(t,"<b>komnata4</b>\nSaved in version 1.27.18.","<b>komnata4</b>\nСохранено в версии 1.27.18.");
  Eq(t,"<b>1 limbs selected</b>\n<color=#FF5555></color>23% burnt\n9% skin damage\n","<b>Выбрано конечностей: 1</b>\n<color=#FF5555></color>Ожоги: 23%\nПовреждение кожи: 9%\n");
  Eq(t,"</color>2 gunshot wounds","</color>Огнестрельные раны: 2"); Eq(t,"12 bruises","Синяки: 12"); Eq(t,"-3.5 m","-3.5 м");
  Eq(t,"Alpha7","7"); Eq(t,"Mouse0","ЛКМ"); Eq(t,"Pasted 15 objects","Вставлено объектов: 15");
  Eq(t,"Now playing <color=#ff54f9>Hungarian Dance No5</color>","Сейчас играет <color=#ff54f9>Hungarian Dance No5</color>");
  Eq(t,"version <b>1.27.18 <color=green>[SAFE MODE]</color> (64 bit)</b>","версия <b>1.27.18 <color=green>[БЕЗОПАСНЫЙ РЕЖИМ]</color> (64 бит)</b>");
  Eq(t,"Enter Tiny","Играть: Крошечная"); Eq(t,"Enter something weird","Enter something weird");
  Eq(t,"<size=80%>Edit the current environment settings\\nSome maps don't support some settings</size>","<size=80%>Изменить настройки окружения на этой карте\\nНекоторые карты поддерживают не все настройки</size>");
  Eq(t,"Copies the selected objects","Копирует выделенные объекты"); Eq(t,"Press E to open the door","Нажмите E, чтобы open the door"); Eq(t,"Angle snap set to 15 degrees","Шаг поворота: 15°"); Eq(t,"Copied 3 objects","Скопировано объектов: 3"); Eq(t,"COAGULATION SERUM","СЫВОРОТКА СВЁРТЫВАНИЯ");
  Eq(t,"IDLE","ПОКОЙ"); Eq(t,"DRAIN","СЛИВ"); Eq(t,"PULL","ВТЯГИВАНИЕ"); Eq(t,"REACTOR IS SUBCRITICAL","РЕАКТОР: ПОДКРИТИЧЕСКИЙ"); Eq(t,"REACTOR IS WEIRD","REACTOR IS WEIRD");
  Eq(t,"<align=\"center\"><#fff>LIQUID CONTENTS\n</align></color>\n\n70% BLOOD\n30% LIFE SERUM\n","<align=\"center\"><#fff>СОСТАВ ЖИДКОСТИ\n</align></color>\n\n70% КРОВЬ\n30% СЫВОРОТКА ЖИЗНИ\n"); Eq(t,"<i>Mixture</i>\n3L / 5L","<i>Смесь</i>\n3L / 5L");
  Eq(t,"Escape","Esc"); Eq(t,"Esc","Esc"); Eq(t,"Totally unknown string here","Totally unknown string here");
  var sw=System.Diagnostics.Stopwatch.StartNew(); for(int i=0;i<1000000;i++) t.Translate("A sharp metal sword."); Console.WriteLine("1M cached lookups ms: "+sw.ElapsedMilliseconds);
  sw.Restart(); for(int i=0;i<20000;i++) t.Translate("Dynamic value "+i+"\nKnife <b>x</b>"); Console.WriteLine("20k uncached ms: "+sw.ElapsedMilliseconds);
  t.FlushDump(); Console.WriteLine("dump lines: "+System.IO.File.ReadAllLines(a[1]).Length);
  Console.WriteLine(fail==0?"ALL PASSED":"FAILED: "+fail); }}
