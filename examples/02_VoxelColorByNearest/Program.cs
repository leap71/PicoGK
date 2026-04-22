using PicoGK;
using System.Numerics;

// Referenzpunkte: jeder Voxel/Dreieck bekommt die Farbe des nächstgelegenen Punktes
Vector3[] aReferenzpunkte =
[
    new Vector3(-4f,  0f,  0f),   // links
    new Vector3( 4f,  0f,  0f),   // rechts
    new Vector3( 0f, -4f,  0f),   // vorne
    new Vector3( 0f,  4f,  0f),   // hinten
    new Vector3( 0f,  0f,  3f),   // oben
    new Vector3( 0f,  0f, -3f),   // unten
];

ColorFloat[] aFarben =
[
    new ColorFloat("FF4444"),   // Rot   — links
    new ColorFloat("44AAFF"),   // Blau  — rechts
    new ColorFloat("44FF88"),   // Grün  — vorne
    new ColorFloat("FFAA44"),   // Orange— hinten
    new ColorFloat("FFFF44"),   // Gelb  — oben
    new ColorFloat("CC44FF"),   // Lila  — unten
];

Library.Go(
    0.05f,
    () =>
    {
        // Geometrie: Kugel + überlappender Quader
        Lattice lat = new();
        lat.AddSphere(Vector3.Zero, 3f);
        lat.AddBeam(
            new Vector3(-5f, 0f, 0f), 0.8f,
            new Vector3( 5f, 0f, 0f), 0.8f,
            true);

        Voxels vox = new Voxels(lat);
        Mesh mshGesamt = vox.mshAsMesh();

        Library.Log($"Mesh: {mshGesamt.nTriangleCount()} Dreiecke, {mshGesamt.nVertexCount()} Vertices");

        // Dreiecke nach nächstem Referenzpunkt aufteilen
        Mesh[] aMeshProPunkt = new Mesh[aReferenzpunkte.Length];
        for (int i = 0; i < aMeshProPunkt.Length; i++)
            aMeshProPunkt[i] = new Mesh();

        int nDreiecke = mshGesamt.nTriangleCount();
        for (int iDreieck = 0; iDreieck < nDreiecke; iDreieck++)
        {
            mshGesamt.GetTriangle(iDreieck,
                out Vector3 vecA, out Vector3 vecB, out Vector3 vecC);

            Vector3 vecZentrum = (vecA + vecB + vecC) / 3f;

            int iBester     = 0;
            float fMinDist  = float.MaxValue;

            for (int k = 0; k < aReferenzpunkte.Length; k++)
            {
                float fDist = Vector3.Distance(vecZentrum, aReferenzpunkte[k]);
                if (fDist < fMinDist)
                {
                    fMinDist = fDist;
                    iBester  = k;
                }
            }

            aMeshProPunkt[iBester].nAddTriangle(vecA, vecB, vecC);
        }

        Viewer oViewer = Library.oViewer();

        // Jede Gruppe mit eigener Farbe zum Viewer hinzufügen
        for (int i = 0; i < aMeshProPunkt.Length; i++)
        {
            int nGroup = i + 1;   // Gruppe 0 = Standard, daher ab 1
            oViewer.Add(aMeshProPunkt[i], nGroup);
            oViewer.SetGroupMaterial(nGroup, aFarben[i], 0.1f, 0.4f);

            Library.Log($"Gruppe {nGroup}: {aMeshProPunkt[i].nTriangleCount()} Dreiecke → {aFarben[i]}");
        }

        // Referenzpunkte als kleine Kugeln visualisieren
        for (int i = 0; i < aReferenzpunkte.Length; i++)
        {
            Lattice latPunkt = new();
            latPunkt.AddSphere(aReferenzpunkte[i], 0.2f);
            Voxels voxPunkt = new Voxels(latPunkt);
            Mesh mshPunkt = voxPunkt.mshAsMesh();

            int nGroup = 100 + i;
            oViewer.Add(mshPunkt, nGroup);
            oViewer.SetGroupMaterial(nGroup, aFarben[i], 0.9f, 0.1f);
        }

        Library.Log("Voxel-Einfärbung nach nächstem Punkt abgeschlossen.");
    }
);
