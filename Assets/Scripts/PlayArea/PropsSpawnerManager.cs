using System.CodeDom.Compiler;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEngine;

public class PropsSpawnerManager : MonoBehaviour
{
    [UsedImplicitly]
    private void Start ()
    {
        var rnd = new System.Random();
        var spawners = Resources.FindObjectsOfTypeAll<PropsSpawner>().ToList();
        for (var i = 0; i < CodeGenerator.RequiredPhoneNumberDigits; i++)
        {
            var index = rnd.Next(spawners.Count);
            spawners[index].SetCode("");
            spawners.RemoveAt(index);
        }
	}

    private string GenerateCode(int index, int count, int digit)
    {
        var code = new StringBuilder(count);

        for (var i = 0; i < index; i++)
            code.Append('*');

        code.Append(digit);

        for (var i = index + 1; i < count; i++)
            code.Append('*');


        return code.ToString();
    }
}
