using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways, DefaultExecutionOrder(195)]
public class SpeechBubbleGraphic : MaskableGraphic
{
    [SerializeField] private RectTransform bubbleOutline;
    [SerializeField] private RectTransform tail;
    [SerializeField] private RectTransform tailOutline;
    [SerializeField] private Color outlineColor = Color.black;

    private const AdditionalCanvasShaderChannels RequiredChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
    
    private static readonly Vector2[] unitCorners = { new(0f, 0f), new(0f, 1f), new(1f, 1f), new(1f, 0f) };

    private readonly Matrix4x4[] lastMappings = new Matrix4x4[3];
    private float opacity = 1f;
    private Color? outlineOverride;

    public float Opacity
    {
        get => opacity;
        set
        {
            opacity = value;
            SetVerticesDirty();
        }
    }

    // null means authored outline color
    public Color? OutlineOverride
    {
        set
        {
            outlineOverride = value;
            SetVerticesDirty();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if ((canvas.additionalShaderChannels & RequiredChannels) != RequiredChannels)
        {
            canvas.additionalShaderChannels |= RequiredChannels;
        }
    }
    
    private void LateUpdate()
    {
        bool changed = CacheMapping(0, bubbleOutline);
        changed |= CacheMapping(1, tail);
        changed |= CacheMapping(2, tailOutline);
        if (changed)
        {
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect bounds = rectTransform.rect;
        Encapsulate(ref bounds, bubbleOutline);
        Encapsulate(ref bounds, tail);
        Encapsulate(ref bounds, tailOutline);

        Matrix4x4 bubbleUV = UVMapping(rectTransform);
        Matrix4x4 bubbleOutlineUV = UVMapping(bubbleOutline);
        Matrix4x4 tailUV = UVMapping(tail);
        Matrix4x4 tailOutlineUV = UVMapping(tailOutline);

        Color fill = color;
        fill.a *= opacity;
        
        Color outline = outlineOverride ?? outlineColor;
        if (QualitySettings.activeColorSpace == ColorSpace.Linear)
        {
            outline = outline.linear;
        }

        UIVertex vertex = UIVertex.simpleVert;
        foreach (Vector2 corner in unitCorners)
        {
            Vector3 p = new(Mathf.Lerp(bounds.xMin, bounds.xMax, corner.x), Mathf.Lerp(bounds.yMin, bounds.yMax, corner.y));
            vertex.position = p;
            vertex.color = fill;
            vertex.uv0 = UVPair(bubbleUV, tailUV, p);
            vertex.uv1 = UVPair(bubbleOutlineUV, tailOutlineUV, p);
            vertex.uv2 = outline;
            vh.AddVert(vertex);
        }

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }

    private bool CacheMapping(int index, RectTransform target)
    {
        Matrix4x4 mapping = UVMapping(target);
        if (mapping == lastMappings[index])
        {
            return false;
        }

        lastMappings[index] = mapping;
        return true;
    }

    // this graphic's local space -> target's normalized rect coords, unclamped 
    private Matrix4x4 UVMapping(RectTransform target)
    {
        Rect r = target.rect;
        Matrix4x4 normalize = Matrix4x4.Scale(new Vector3(1f / r.width, 1f / r.height, 1f)) * Matrix4x4.Translate(-r.min);
        return normalize * target.worldToLocalMatrix * rectTransform.localToWorldMatrix;
    }

    private void Encapsulate(ref Rect bounds, RectTransform target)
    {
        Matrix4x4 toLocal = rectTransform.worldToLocalMatrix * target.localToWorldMatrix;
        Rect r = target.rect;
        foreach (Vector2 corner in unitCorners)
        {
            Vector3 p = toLocal.MultiplyPoint3x4(Rect.NormalizedToPoint(r, corner));
            bounds.xMin = Mathf.Min(bounds.xMin, p.x);
            bounds.xMax = Mathf.Max(bounds.xMax, p.x);
            bounds.yMin = Mathf.Min(bounds.yMin, p.y);
            bounds.yMax = Mathf.Max(bounds.yMax, p.y);
        }
    }

    private static Vector4 UVPair(Matrix4x4 a, Matrix4x4 b, Vector3 p)
    {
        Vector3 uvA = a.MultiplyPoint3x4(p);
        Vector3 uvB = b.MultiplyPoint3x4(p);
        return new Vector4(uvA.x, uvA.y, uvB.x, uvB.y);
    }
}
