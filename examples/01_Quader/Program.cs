using PicoGK;
using System.Numerics;

// Quader: L=6mm, B=0.25mm, H=3mm, zentriert um den Ursprung
float fL = 6f;
float fB = 0.25f;
float fH = 3f;

Library.Go(
    0.05f,
    () =>
    {
        BBox3 oBounds = new BBox3(
            new Vector3(-fL / 2f, -fB / 2f, -fH / 2f),
            new Vector3( fL / 2f,  fB / 2f,  fH / 2f)
        );

        Voxels vox = new Voxels(new QuaderSDF(oBounds));

        Mesh msh = vox.mshAsMesh();
        msh.SaveToStlFile("Quader_6x025x3.stl");
        Library.Log("STL gespeichert: Quader_6x025x3.stl");

        Library.oViewer().Add(vox);
    }
);

/// <summary>
/// Achsenparalleler Quader als SDF — negativ innen, positiv außen.
/// </summary>
class QuaderSDF : IBoundedImplicit
{
    public QuaderSDF(BBox3 oBounds)
    {
        m_oBounds  = oBounds;
        m_vecHalf  = (oBounds.vecMax - oBounds.vecMin) / 2f;
        m_vecCenter = (oBounds.vecMin + oBounds.vecMax) / 2f;
    }

    public BBox3 oBounds => m_oBounds;

    public float fSignedDistance(in Vector3 vec)
    {
        // Abstand zur nächsten Quader-Oberfläche (Box-SDF)
        Vector3 vecQ = Vector3.Abs(vec - m_vecCenter) - m_vecHalf;
        return Vector3.Max(vecQ, Vector3.Zero).Length()
             + MathF.Min(MathF.Max(vecQ.X, MathF.Max(vecQ.Y, vecQ.Z)), 0f);
    }

    readonly BBox3   m_oBounds;
    readonly Vector3 m_vecHalf;
    readonly Vector3 m_vecCenter;
}
