namespace DiskLens;
public static class MapPalette {
 static readonly string[] Folders=["#397D73","#3B699B","#78599C","#A35B6F","#94703D","#527C47","#426F85","#895978","#866B4E","#56669C","#387C80","#9B487B"];
 public static Dictionary<string,string> FolderColors(IEnumerable<Entry> entries){var result=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);var used=new HashSet<int>();foreach(var path in entries.Where(e=>e.Kind=="folder").Select(e=>e.Path).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(p=>p,StringComparer.OrdinalIgnoreCase)){uint hash=2166136261;foreach(var c in path.ToUpperInvariant()){hash^=c;hash*=16777619;}var index=(int)(hash%(uint)Folders.Length);if(used.Count<Folders.Length){while(used.Contains(index))index=(index+1)%Folders.Length;used.Add(index);}result[path]=Folders[index];}return result;}
 public static string FileColor(Entry e)=>e.Kind=="remainder"?"#59636A":Rules.Category(e.Path) switch {"Video"=>"#996B27","Images"=>"#337C59","Archives"=>"#9A4B61","Code"=>"#775596","Documents"=>"#346F8B","Applications" or "Installers"=>"#4568A3",_=>"#59636A"};
}

