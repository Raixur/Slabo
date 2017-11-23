using TMPro;
using UnityEngine;

public class PropsSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshPro textCode = null;

    public void SetCode(string code)
    {
        textCode.SetText(code);
    }
}
