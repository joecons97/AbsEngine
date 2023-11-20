using AbsEngine.ECS.Components;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Diagnostics;
using System.Numerics;

namespace AbsEngine.ECS.Systems;

public class FlyCamSystem : AsyncComponentSystem<CameraComponent>
{
    Vector2 lastPos;
    IInputContext inputContext;
    IMouse mouse;
    IKeyboard keyboard;

    public float MoveSpeed { get; private set; } = 15;

    protected override Func<CameraComponent, bool>? Predicate => (x) =>
    {
        return x.IsMainCamera;
    };

    public FlyCamSystem(Scene scene) : base(scene)
    {
        inputContext = scene.Game.InputContext;
        mouse = inputContext.Mice.First();
        keyboard = inputContext.Keyboards.First();

        mouse.Cursor.CursorMode = CursorMode.Disabled;
    }

    public void OnTick(CameraComponent component, float deltaTime)
    {
        var t = component.Entity.Transform;

        Debug.WriteLine(component.Entity.Transform.LocalPosition.ToString());

        var p = mouse.Position;
        if (lastPos == default)
            lastPos = p;

        Vector3D<float> velocity = new Vector3D<float>();

        if (keyboard.IsKeyPressed(Key.W))
        {
            velocity += t.Forward * MoveSpeed * deltaTime;
        }
        else if (keyboard.IsKeyPressed(Key.S))
        {
            velocity += t.Forward * -MoveSpeed * deltaTime;
        }

        if (keyboard.IsKeyPressed(Key.D))
        {
            velocity += t.Right * -MoveSpeed * deltaTime;
        }
        else if (keyboard.IsKeyPressed(Key.A))
        {
            velocity += t.Right * MoveSpeed * deltaTime;
        }

        if(keyboard.IsKeyPressed(Key.Equal))
        {
            SceneCameraComponent.IsInSceneView = true;
        }

        var d = lastPos - p;
        lastPos = p;

        float m = 0.1f;

        t.LocalEulerAngles += new Vector3D<float>(d.Y * m, d.X * m, 0);
        t.LocalPosition += velocity;
    }

    public override Task OnTickAsync(CameraComponent component, float deltaTime)
    {
        if (!SceneCameraComponent.IsInSceneView)
            OnTick(component, deltaTime);

        return Task.CompletedTask;
    }
}
