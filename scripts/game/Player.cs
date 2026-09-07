using Godot;

namespace BloodDragon
{
    /// <summary>
    /// Minimal first-person controller. Uses the remappable input actions and
    /// reads mouse sensitivity / invert-Y / FOV from the live settings so the
    /// settings menu has something tangible to drive.
    /// </summary>
    public partial class Player : CharacterBody3D
    {
        private const float Gravity = 22f;
        private const float JumpVelocity = 7.5f;
        private const float WalkSpeed = 5f;
        private const float SprintSpeed = 8.5f;
        private const float CrouchSpeed = 2.5f;
        private const float StandHeight = 1.8f;
        private const float CrouchHeight = 1.0f;

        public Camera3D Camera { get; private set; }
        private Node3D _head;
        private CollisionShape3D _capsule;
        private float _pitch;
        private bool _crouching;

        public override void _Ready()
        {
            _capsule = new CollisionShape3D
            {
                Shape = new CapsuleShape3D { Height = StandHeight, Radius = 0.35f },
                Position = new Vector3(0, 0.9f, 0),
            };
            AddChild(_capsule);

            _head = new Node3D { Position = new Vector3(0, 1.6f, 0) };
            AddChild(_head);

            Camera = new Camera3D { Current = true };
            _head.AddChild(Camera);

            RefreshFromSettings();
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Applied += RefreshFromSettings;

            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        public override void _ExitTree()
        {
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.Applied -= RefreshFromSettings;
        }

        private void RefreshFromSettings()
        {
            if (Camera != null && SettingsManager.Instance != null)
                Camera.Fov = SettingsManager.Instance.Current.Fov;
        }

        public override void _Input(InputEvent @event)
        {
            if (GetTree().Paused) return;
            if (@event is InputEventMouseMotion m && Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                var s = SettingsManager.Instance.Current;
                float sens = 0.003f * Mathf.Lerp(0.3f, 2.5f, s.MouseSensitivity);
                float invert = s.InvertY ? -1f : 1f;

                RotateY(-m.Relative.X * sens);
                _pitch = Mathf.Clamp(_pitch - m.Relative.Y * sens * invert, -1.4f, 1.4f);
                _head.Rotation = new Vector3(_pitch, 0, 0);
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (GetTree().Paused) return;

            bool wantCrouch = Input.IsActionPressed("crouch");
            if (wantCrouch != _crouching)
            {
                _crouching = wantCrouch;
                float h = _crouching ? CrouchHeight : StandHeight;
                ((CapsuleShape3D)_capsule.Shape).Height = h;
                _capsule.Position = new Vector3(0, h / 2f, 0);
                _head.Position = new Vector3(0, _crouching ? 0.9f : 1.6f, 0);
            }

            Vector3 v = Velocity;
            if (!IsOnFloor())
                v.Y -= Gravity * (float)delta;
            else if (Input.IsActionJustPressed("jump") && !_crouching)
                v.Y = JumpVelocity;

            Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            Vector3 dir = (Transform.Basis * new Vector3(input.X, 0, input.Y)).Normalized();
            float speed = _crouching ? CrouchSpeed
                       : Input.IsActionPressed("sprint") ? SprintSpeed
                       : WalkSpeed;

            if (dir != Vector3.Zero)
            {
                v.X = dir.X * speed;
                v.Z = dir.Z * speed;
            }
            else
            {
                v.X = Mathf.MoveToward(v.X, 0, speed);
                v.Z = Mathf.MoveToward(v.Z, 0, speed);
            }

            Velocity = v;
            MoveAndSlide();
        }
    }
}
