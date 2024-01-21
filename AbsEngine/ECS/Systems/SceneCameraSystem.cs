using AbsEngine.ECS.Components;
using ImGuiNET;
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

    bool canMove;

    public float MoveSpeed { get; private set; } = 15;

    public SceneCameraSystem(Scene scene) : base(scene)
    {
        inputContext = scene.Game.InputContext;
        mouse = inputContext.Mice.First();
        keyboard = inputContext.Keyboards.First();

        mouse.MouseDown += Mouse_MouseDown;
        mouse.MouseUp += Mouse_MouseUp;
    }

    private void Mouse_MouseUp(IMouse arg1, MouseButton arg2)
    {
        if (!SceneCameraComponent.IsInSceneView) return;

        if (arg2 == MouseButton.Right)
        {
            mouse.Cursor.CursorMode = CursorMode.Normal;
            canMove = false;
        }
    }

    private void Mouse_MouseDown(IMouse arg1, MouseButton arg2)
    {
        if (!SceneCameraComponent.IsInSceneView) return;

        if (arg2 == MouseButton.Right)
        {
            mouse.Cursor.CursorMode = CursorMode.Disabled;
            canMove = true;
        }
    }

    public void OnTick(SceneCameraComponent component, float deltaTime)
    {
        var t = component.Entity.Transform;

        var p = mouse.Position;
        if (lastPos == default)
            lastPos = p;

        Vector3D<float> velocity = new Vector3D<float>();

        if (canMove)
        {
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
        }

        if (keyboard.IsKeyPressed(Key.Minus))
        {
            SceneCameraComponent.IsInSceneView = false;
        }

        var d = lastPos - p;
        lastPos = p;

        float m = 0.1f;

        if (canMove)
        {
            t.LocalEulerAngles += new Vector3D<float>(d.Y * m, d.X * m, 0);
            t.LocalPosition += velocity;
        }
    }

    public override Task OnTickAsync(SceneCameraComponent component, float deltaTime)
    {
        if (component.IsMainCamera && SceneCameraComponent.IsInSceneView)
        {
            OnTick(component, deltaTime);
        }

        return Task.CompletedTask;
    }
}
