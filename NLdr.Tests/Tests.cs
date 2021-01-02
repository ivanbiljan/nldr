using System;
using System.IO;
using Xunit;

namespace NLdr.Tests {
    public interface LuaInterface {
        IntPtr lua_newstate();
    }
    
    public class Tests {
        [Fact]
        public void Test1() {
            Assert.True(File.Exists("lua53.dll"));
            var luaInterface = NativeLibrary.Default.Bind<LuaInterface>("lua53.dll");
            Assert.True(luaInterface.lua_newstate() != default);
        }
    }
}