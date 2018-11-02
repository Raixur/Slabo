using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class PropsSpawner : MonoBehaviour
{
    [SerializeField] private TextMeshPro textCode = null;

    [UsedImplicitly]
    private void Awake()
    {
        textCode.gameObject.SetActive(false);
    }

    public void SetCode(string code)
    {
        textCode.gameObject.SetActive(true);
        textCode.SetText(code);
    }
}
