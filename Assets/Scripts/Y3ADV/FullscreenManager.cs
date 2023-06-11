#if WEBGL_BUILD
namespace Y3ADV
{
    public class FullscreenManager : SingletonMonoBehaviour<FullscreenManager>
    {
        private bool fullscreen = false;
        public bool Fullscreen => fullscreen;

        private void Start()
        {
            WebGLInterops.RegisterFullscreenManagerGameObject(gameObject.name);
        }

        public void ChangeFullscreen(int status)
        {
            fullscreen = status > 0;
        }
    }
}
#endif