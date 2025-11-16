using UnityEngine.UI;
using UnityEngine;

public class Pointer : MonoBehaviour
{
    public static Pointer Instance { get; private set; }
    public Node Node { get; private set; }
    public MonoBehaviour RegisteredObject { get; private set; } // Node または SequentNode など
    
    // 最後に操作された InferenceBar
    public InferenceBar LastInferenceBar { get; private set; }
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
            if(Node != null)
                Destroy(Node.gameObject);
            if(RegisteredObject != null)
                Destroy(RegisteredObject.gameObject);
            Unregister();
        }
    }

    public void Register(Node node)
    {
        // 既に異なるノードが登録されている場合は前のものをアンレジスター
        if (this.Node != null && this.Node != node)
        {
            Debug.Log("Unregistering previous node to register new node");
            var img = this.Node.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
            this.Node = null;
        }
        
        // 同じノードが既に登録されている場合はスキップ
        if (this.Node == node)
        {
            Debug.Log("Node already registered in Pointer");
            return;
        }
        
        Debug.Log("Node registered in Pointer");
        this.Node = node;
        this.RegisteredObject = node;
        var newImg = node.GetComponent<Image>();
        if (newImg != null) newImg.raycastTarget = false;
    }

    /// <summary>
    /// MonoBehaviour を Pointer に登録する（SequentNode など）
    /// </summary>
    public void Register(MonoBehaviour obj)
    {
        // 既に異なるオブジェクトが登録されている場合は前のものをアンレジスター
        if (this.RegisteredObject != null && this.RegisteredObject != obj)
        {
            Debug.Log("Unregistering previous object to register new object");
            var img = this.RegisteredObject.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
            this.RegisteredObject = null;
            this.Node = null;
        }
        
        // 同じオブジェクトが既に登録されている場合はスキップ
        if (this.RegisteredObject == obj)
        {
            Debug.Log($"Object already registered in Pointer: {obj.GetType().Name}");
            return;
        }
        
        Debug.Log($"Object registered in Pointer: {obj.GetType().Name}");
        this.RegisteredObject = obj;
        var newImg = obj.GetComponent<Image>();
        if (newImg != null) newImg.raycastTarget = false;
    }
    
    public void Unregister()
    {
        if(this.Node != null)
        {
            var img = Node.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
            Node = null;
        }
        if(this.RegisteredObject != null)
        {
            var img = RegisteredObject.GetComponent<Image>();
            if (img != null) img.raycastTarget = true;
            RegisteredObject = null;
        }
    }

    // 最後に触った InferenceBar を登録
    public void RegisterInferenceBar(InferenceBar bar)
    {
        if (bar == null) return;
        LastInferenceBar = bar;
        Debug.Log($"[Pointer] LastInferenceBar set to {bar.name}");
    }
}
