using System.Runtime.InteropServices;
using System.Text;
namespace DiskLens;
public sealed partial class Db {
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_bind_text(IntPtr statement,int index,[MarshalAs(UnmanagedType.LPUTF8Str)]string value,int length,IntPtr destructor);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_bind_int64(IntPtr statement,int index,long value);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_reset(IntPtr statement);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_clear_bindings(IntPtr statement);
 [DllImport("winsqlite3",CallingConvention=CallingConvention.Cdecl)] static extern int sqlite3_changes(IntPtr connection);
 public Statement Prepare(string sql)=>new(this,sql);
 public sealed class Statement:IDisposable {
  readonly Db owner;IntPtr handle;
  internal Statement(Db owner,string sql){this.owner=owner;var bytes=Encoding.UTF8.GetBytes(sql+"\0");if(sqlite3_prepare_v2(owner.db,bytes,bytes.Length,out handle,IntPtr.Zero)!=0)throw Error();}
  IOException Error()=>new(Marshal.PtrToStringUTF8(sqlite3_errmsg(owner.db)));
  public void Text(int index,string value){if(sqlite3_bind_text(handle,index,value,-1,new IntPtr(-1))!=0)throw Error();}
  public void Number(int index,long value){if(sqlite3_bind_int64(handle,index,value)!=0)throw Error();}
  public int Execute(){try{if(sqlite3_step(handle)!=101)throw Error();return sqlite3_changes(owner.db);}finally{sqlite3_reset(handle);sqlite3_clear_bindings(handle);}}
  public void Dispose(){if(handle!=IntPtr.Zero){sqlite3_finalize(handle);handle=IntPtr.Zero;}}
 }
}
