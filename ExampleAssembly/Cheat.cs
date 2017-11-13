namespace ExampleAssembly
{
    public class Cheat : UnityEngine.MonoBehaviour
    {
        private void OnGUI()
        {
            UnityEngine.GUI.Label(new UnityEngine.Rect(10, 10, 100, 20), "This is a very useful cheat");
        }
    }
}
