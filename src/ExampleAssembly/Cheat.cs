namespace ExampleAssembly
{
    public class Cheat : UnityEngine.MonoBehaviour
    {
        private void OnGUI()
        {
            UnityEngine.GUI.Label(new UnityEngine.Rect(10, 10, 200, 40), "This is a very useful cheat");
        }
    }
}
