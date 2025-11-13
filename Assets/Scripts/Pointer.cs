using UnityEngine.UI;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    public static Pointer Instance { get; private set; }
    public Node Node { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            Destroy(Node.gameObject);
            Unregister();
        }
    }

    public void Register(Node node)
    {
        if (this.Node != null)
        {
            Debug.LogError("Pointer already has a registered node");
            return;
        }
        Debug.Log("Node registered in Pointer");
        this.Node = node;
        Node.GetComponent<Image>().raycastTarget = false;
    }
    
    public void Unregister()
    {
        if(this.Node == null)
        {
            return;
        }
        Node.GetComponent<Image>().raycastTarget = true;
        Node = null;
        
    }
}
