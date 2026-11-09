using System.Runtime.InteropServices;
using System.Text;
namespace DiskLens;
// Windows ships winsqlite3: no NuGet/native SQLite payload is distributed.
public sealed partial class Db : IDisposable {
 IntPtr db;
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_open([MarshalAs(UnmanagedType.LPUTF8Str)] string p,out IntPtr db);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_close(IntPtr db);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_exec(IntPtr db,[MarshalAs(UnmanagedType.LPUTF8Str)] string sql,IntPtr cb,IntPtr arg,out IntPtr err);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern IntPtr sqlite3_errmsg(IntPtr db);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_prepare_v2(IntPtr db,byte[] sql,int n,out IntPtr stmt,IntPtr tail);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_step(IntPtr stmt);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_finalize(IntPtr stmt);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_column_count(IntPtr stmt);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern IntPtr sqlite3_column_text(IntPtr stmt,int col);
 public Db(string path){if(sqlite3_open(path,out db)!=0)throw new IOException("Cannot open index");Exec("PRAGMA busy_timeout=5000; PRAGMA cache_size=-8192; PRAGMA temp_store=FILE;");}
 public static string Q(string? s)=>"'"+(s??"").Replace("'","''")+"'";
 public void Exec(string sql){if(sqlite3_exec(db,sql,IntPtr.Zero,IntPtr.Zero,out _)!=0)throw new IOException(Marshal.PtrToStringUTF8(sqlite3_errmsg(db)));}
 public IEnumerable<string[]> Query(string sql){var bytes=Encoding.UTF8.GetBytes(sql+"\0");if(sqlite3_prepare_v2(db,bytes,bytes.Length,out var s,IntPtr.Zero)!=0)throw new IOException(Marshal.PtrToStringUTF8(sqlite3_errmsg(db)));try{int rc;while((rc=sqlite3_step(s))==100){var row=new string[sqlite3_column_count(s)];for(int i=0;i<row.Length;i++)row[i]=Marshal.PtrToStringUTF8(sqlite3_column_text(s,i))??"";yield return row;}if(rc!=101)throw new IOException(Marshal.PtrToStringUTF8(sqlite3_errmsg(db)));}finally{sqlite3_finalize(s);}}
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern void sqlite3_interrupt(IntPtr db); public void Interrupt()=>sqlite3_interrupt(db); public void Dispose(){sqlite3_close(db);db=IntPtr.Zero;}
}


