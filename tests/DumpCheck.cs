using System; using System.IO; using System.Text.RegularExpressions; using PPGTranslate;
class D { static string U(string s){ return s.Replace("\\\\","\u0001").Replace("\\n","\n").Replace("\u0001","\\"); }
 static string Sh(string s){ return s.Replace("\n","⏎"); }
 static void Main(string[] a){ var t=new Translator(); foreach(var sub in new[]{"_all",a.Length>3?a[3]:"ru"}){ var fs=Directory.GetFiles(Path.Combine(a[0],sub),"*.txt"); Array.Sort(fs,StringComparer.OrdinalIgnoreCase); foreach(var f in fs) t.LoadFile(f); }
  int all=0, full=0, part=0, none=0;
  foreach(var raw in File.ReadAllLines(a[1])){ if(raw.StartsWith("#")||raw.Trim()=="") continue; var k=U(raw.Split('\t')[0]); all++;
    var r=t.Translate(k); var plain=Regex.Replace(r,"<[^>]*>","");
    bool eng=Regex.IsMatch(plain,"[A-Za-z]{3,}"); bool changed=r!=k;
    if(!changed){none++; Console.WriteLine("НЕТ   "+Sh(k));}
    else if(eng){part++; Console.WriteLine("ЧАСТЬ "+Sh(k)+"\n   => "+Sh(r));}
    else full++; if(a.Length>2) Console.WriteLine("  "+Sh(k)+" => "+Sh(r)); }
  Console.WriteLine("всего "+all+", полностью "+full+", частично "+part+", без изменений "+none); }}
