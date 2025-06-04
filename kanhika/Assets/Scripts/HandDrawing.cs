using UnityEngine;

public class HandDrawing : MonoBehaviour
{
    [SerializeField] private OVRHand hand;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private Material brushMaterial;
    [SerializeField] private Collider drawSurface;
    [SerializeField] private GameObject brushPrefab;

    [SerializeField] private float brushSize = 0.03f;
    [SerializeField] private float maxDistance = 1f;

    private bool _wasPinching;

    private void Update()
    {
        bool isPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
        if (isPinching)
        {
            var ray = new Ray(brushPrefab.transform.position, brushPrefab.transform.forward);
            if (drawSurface.Raycast(ray, out var hit, maxDistance))
            {
                var uv = hit.textureCoord;
                DrawAtUV(uv);
            }
        }

        _wasPinching = isPinching;
    }

    private void DrawAtUV(Vector2 uv)
    {
        Debug.Log("Drawing at UV: " + uv);
        Graphics.SetRenderTarget(renderTexture);
        GL.PushMatrix();
        GL.LoadOrtho();
        brushMaterial.SetPass(0);
        GL.Begin(GL.QUADS);
        GL.TexCoord2(0, 0);
        GL.Vertex3(uv.x - brushSize, uv.y - brushSize, 0);
        GL.TexCoord2(0, 1);
        GL.Vertex3(uv.x - brushSize, uv.y + brushSize, 0);
        GL.TexCoord2(1, 1);
        GL.Vertex3(uv.x + brushSize, uv.y + brushSize, 0);
        GL.TexCoord2(1, 0);
        GL.Vertex3(uv.x + brushSize, uv.y - brushSize, 0);
        GL.End();
        GL.PopMatrix();
        Graphics.SetRenderTarget(null);
    }

    public void ChangeTexture(RenderTexture newTexture)
    {
        renderTexture = newTexture;
    }

    public void ChangeCollider(Collider newCollider)
    {
        drawSurface = newCollider;
    }


    public void ClearDrawing()
    {
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, new Color(215f, 202f, 179f, 255));
        RenderTexture.active = null;
    }

    // private void OnDrawGizmos()
    // {
    //     Ray ray = new Ray(brushPrefab.transform.position,
    //         brushPrefab.transform.forward);
    //     if (drawSurface.Raycast(ray, out RaycastHit hit, maxDistance))
    //     {
    //         Gizmos.color = Color.blue;
    //         Gizmos.DrawSphere(hit.point, brushSize);
    //     }
    // }
}