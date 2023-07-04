using UnityEngine;

// Credits: Zain Al Rubaie

public enum Surface //the different walking surface types
{
    //add new sounds at the bottom
    Wood,
    Carpet,
    Tile
        
}
public class SoundSurfaceIDIdentifier : MonoBehaviour
{
    
    [SerializeField] private Surface _surface; //allows you to choose a surface type

    public Surface Surface
    {
        get { return _surface; }
        set { _surface = value; }
    }


}
