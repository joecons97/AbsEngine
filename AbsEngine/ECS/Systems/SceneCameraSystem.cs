using AbsEngine.ECS.Components;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Numerics;

namespace AbsEngine.ECS.Systems;

public class SceneCameraSystem : AsyncComponentSystem<SceneCameraComponent>
{
    Vector2 lastPos;
    IInputContext inputContext;
    IMouse mouse;
    IKeyboard keyboard;

    public float MoveSpeed { get; private set; } = 15;

    protected override Func<SceneCameraComponent, bool>? Predicate => (x) =>
    {
        return x.IsMainCamera;
    };

    public SceneCameraSystem(Scene scene) : base(scene)
    {
        inputContext = scene.Game.InputContext;
        mouse = inputContext.Mice.First();
        keyboard = inputContext.Keyboards.First();

        mouse.Cursor.CursorMode = CursorMode.Disabled;
    }

    public void OnTick(SceneCameraComponent component, float deltaTime)
    {
        var t = component.Entity.Transform;

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
        else if (keyboard.IsKeyPressed(Key.Minus))
        {
            SceneCameraComponent.IsInSceneView = false;
        }

        if (keyboard.IsKeyPressed(Key.Minus))
        {
            SceneCameraComponent.IsInSceneView = false;
        }

        var d = lastPos - p;
        lastPos = p;

        float m = 0.1f;

        t.LocalEulerAngles += new Vector3D<float>(d.Y * m, d.X * m, 0);
        t.LocalPosition += velocity;
    }

    public override Task OnTickAsync(SceneCameraComponent component, float deltaTime)
    {
        if(SceneCameraComponent.IsInSceneView)
            OnTick(component, deltaTime);

        return Task.CompletedTask;
    }
}
