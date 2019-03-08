using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ArchetypesUnitTests
{
    public class MiniDictTest
    {
        private bool CompareMiniDict<T>(MiniDict<Flags, T> md, Dictionary<Flags, T> nd) 
        {
            foreach (var pair in nd)
                if (!pair.Value.Equals(nd[pair.Key]))
                    return false;
            return true;
        }

        [Fact]
        public void Test()
        {
            const int qty = 20;
            var keys = new Flags[qty];
            for (int i = 0; i < qty; i++)
                keys[i] = new Flags(i);

            var mDict = new MiniDict<Flags, int>(keys);
            var mDict2 = new MiniDict<Flags, int>(keys, Enumerable.Range(0,20).ToArray());
            var nDict = new Dictionary<Flags, int>(qty);

            for (int i = 0; i < qty; i++) mDict[new Flags(i)] = i;
            for (int i = 0; i < qty; i++) nDict[new Flags(i)] = i;

            Assert.True(CompareMiniDict(mDict, nDict));
            Assert.True(CompareMiniDict(mDict2, nDict));
            
            for (int i = 0; i < qty; i++) mDict[new Flags(i)]++;
            for (int i = 0; i < qty; i++) mDict2[new Flags(i)]++;
            for (int i = 0; i < qty; i++) nDict[new Flags(i)]++;

            Assert.True(CompareMiniDict(mDict, nDict));
            Assert.True(CompareMiniDict(mDict2, nDict));
        }
    }
}
