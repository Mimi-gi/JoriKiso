using UnityEngine;
using FormalSystem.LK;

public class NodeCreator : MonoBehaviour
{

    [SerializeField] GameObject[] Variables;
    [SerializeField] GameObject[] Operators;
    int idx = 0;

    public void CreateVariable()
    {
        Instantiate(Variables[idx]);
        idx++;
        idx %= Variables.Length;
    }
    public void CreateNot()
    {
        Instantiate(Operators[0]);
    }
    public void CreateAnd()
    {
        Instantiate(Operators[1]);
    }
    public void CreateOr()
    {
        Instantiate(Operators[2]);
    }
    public void CreateImp()
    {
        Instantiate(Operators[3]);
    }
}
