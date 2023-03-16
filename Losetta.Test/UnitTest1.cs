using AliceScript;

namespace Losetta.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            VariableCollection vc = new VariableCollection();
            vc.Type = new TypeObject(Variable.VarType.STRING);
            vc.Add(new Variable("TEST"));
            try
            {
                vc.Add(new Variable(0.0));
            }
            catch { }

            Assert.IsTrue(vc.Count == 1);
        }
    }
}