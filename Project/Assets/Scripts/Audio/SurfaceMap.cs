using System.Collections.Generic;
using UnityEngine;

// Credits: Zain Al Rubaie

[CreateAssetMenu(menuName = "Audio/Surface Map")]
public class SurfaceMap : ScriptableObject
{
    [SerializeField] private List<SurfacePatchMap> _mapping = new List<SurfacePatchMap>(); //a list of different surfacemaps

    public AudioPatch GetPatch(Surface surface) //gets the audiopatch of a specific surface
    {
        foreach(SurfacePatchMap map in _mapping)
        {
            if(map.surface == surface)
            {
                return map.patch;
            }
        }    
        return null;
    }
}
[System.Serializable]
public struct SurfacePatchMap //allows you to choose a surface type and assign an audio patch to it
{
    public Surface surface;
    public AudioPatch patch;
}