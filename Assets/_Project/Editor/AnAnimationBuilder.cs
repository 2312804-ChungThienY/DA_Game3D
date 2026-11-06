using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Tạo bộ animation blockout cho An (spec phần 43) bằng đường cong muscle Humanoid, vì gói clip có sẵn
// trong dự án (A10 Character_Animated) có rig không map được Humanoid. Chạy lại an toàn: ghi đè asset.
// Locomotion/nhảy giữ clip Starter Assets; thêm layer Actions (nửa thân trên), FullBody (lộn, chết), LanternArm.
public static class AnAnimationBuilder
{
    private const string Folder = "Assets/_Project/Animations";
    private const string SourceController =
        "Assets/A01 - First Person + Third Person  Character Controllers/Runtime/ThirdPersonController/Character/Animations/StarterAssetsThirdPerson.controller";
    private const string ControllerPath = Folder + "/AC_An.controller";
    private const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_An_Prototype.prefab";

    private static readonly Dictionary<string, float> Neutral = new Dictionary<string, float>
    {
        { "Left Arm Down-Up", -0.55f }, { "Right Arm Down-Up", -0.55f },
        { "Left Arm Front-Back", 0.1f }, { "Right Arm Front-Back", 0.1f },
        { "Left Forearm Stretch", 0.8f }, { "Right Forearm Stretch", 0.8f },
        { "Spine Front-Back", 0f }, { "Chest Front-Back", 0f }, { "Chest Twist Left-Right", 0f },
        { "Head Nod Down-Up", 0f }
    };

    [MenuItem("Tools/Son Doong/Build An Animations")]
    public static void Build()
    {
        // Preserve the configured controller GUID and tuned layers on subsequent rebuilds.
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
        {
            MeleeVisualSetup.Rebuild();
            return;
        }
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project", "Animations");

        AnimationClip slash1 = Clip("AN_Slash1", 0.65f, false, new[]
        {
            K(0f, Neutral),
            K(0.15f, M("Right Arm Down-Up", 0.6f), M("Right Arm Front-Back", -0.4f), M("Right Forearm Stretch", 0.2f), M("Chest Twist Left-Right", 0.45f), M("Left Arm Down-Up", -0.55f), M("Left Arm Front-Back", 0.1f), M("Left Forearm Stretch", 0.8f), M("Spine Front-Back", 0f), M("Chest Front-Back", 0f), M("Head Nod Down-Up", 0f)),
            K(0.27f, M("Right Arm Down-Up", 0.1f), M("Right Arm Front-Back", 1f), M("Right Forearm Stretch", 1f), M("Chest Twist Left-Right", -0.5f), M("Spine Front-Back", 0.25f)),
            K(0.45f, M("Right Arm Down-Up", -0.1f), M("Right Arm Front-Back", 0.9f), M("Right Forearm Stretch", 0.9f), M("Chest Twist Left-Right", -0.4f), M("Spine Front-Back", 0.2f)),
            K(0.65f, Neutral)
        });
        AnimationClip slash2 = Clip("AN_Slash2", 0.65f, false, new[]
        {
            K(0f, Neutral),
            K(0.15f, M("Right Arm Down-Up", -0.2f), M("Right Arm Front-Back", 0.9f), M("Right Forearm Stretch", 0.3f), M("Chest Twist Left-Right", -0.45f), M("Left Arm Down-Up", -0.55f), M("Left Arm Front-Back", 0.1f), M("Left Forearm Stretch", 0.8f), M("Spine Front-Back", 0.1f), M("Chest Front-Back", 0f), M("Head Nod Down-Up", 0f)),
            K(0.27f, M("Right Arm Down-Up", 0.25f), M("Right Arm Front-Back", -0.3f), M("Right Forearm Stretch", 1f), M("Chest Twist Left-Right", 0.55f), M("Spine Front-Back", 0.25f)),
            K(0.45f, M("Right Arm Down-Up", 0.1f), M("Right Arm Front-Back", -0.2f), M("Right Forearm Stretch", 0.9f), M("Chest Twist Left-Right", 0.4f), M("Spine Front-Back", 0.2f)),
            K(0.65f, Neutral)
        });
        AnimationClip hit = Clip("AN_Hit", 0.4f, false, new[]
        {
            K(0f, Neutral),
            K(0.1f, M("Spine Front-Back", -0.6f), M("Chest Front-Back", -0.4f), M("Head Nod Down-Up", 0.5f), M("Left Arm Down-Up", -0.2f), M("Right Arm Down-Up", -0.2f), M("Left Arm Front-Back", 0.3f), M("Right Arm Front-Back", 0.3f), M("Left Forearm Stretch", 0.5f), M("Right Forearm Stretch", 0.5f), M("Chest Twist Left-Right", 0f)),
            K(0.4f, Neutral)
        });
        AnimationClip interact = Clip("AN_Interact", 0.5f, false, new[]
        {
            K(0f, Neutral),
            K(0.2f, M("Right Arm Down-Up", -0.15f), M("Right Arm Front-Back", 0.85f), M("Right Forearm Stretch", 0.95f), M("Spine Front-Back", 0.4f), M("Chest Front-Back", 0.2f), M("Head Nod Down-Up", -0.3f), M("Left Arm Down-Up", -0.55f), M("Left Arm Front-Back", 0.1f), M("Left Forearm Stretch", 0.8f), M("Chest Twist Left-Right", 0f)),
            K(0.5f, Neutral)
        });
        AnimationClip lantern = Clip("AN_LanternHold", 1f, true, new[]
        {
            K(0f, M("Left Arm Down-Up", -0.15f), M("Left Arm Front-Back", 0.7f), M("Left Forearm Stretch", 0.15f), M("Left Hand Down-Up", 0.2f)),
            K(1f, M("Left Arm Down-Up", -0.12f), M("Left Arm Front-Back", 0.72f), M("Left Forearm Stretch", 0.15f), M("Left Hand Down-Up", 0.2f))
        });
        var tuck = new[] { M("Spine Front-Back", 1f), M("Chest Front-Back", 1f), M("Head Nod Down-Up", -1f),
            M("Left Upper Leg Front-Back", 1f), M("Right Upper Leg Front-Back", 1f), M("Left Lower Leg Stretch", -1f), M("Right Lower Leg Stretch", -1f),
            M("Left Arm Down-Up", -0.3f), M("Right Arm Down-Up", -0.3f), M("Left Arm Front-Back", 0.7f), M("Right Arm Front-Back", 0.7f),
            M("Left Forearm Stretch", -0.6f), M("Right Forearm Stretch", -0.6f) };
        AnimationClip roll = Clip("AN_Roll", 0.45f, false, new[] { K(0f, tuck), K(0.45f, tuck) });
        AnimationClip death = Clip("AN_Death", 0.8f, false, new[]
        {
            K(0f, M("Spine Front-Back", 0f), M("Left Upper Leg Front-Back", 0f), M("Right Upper Leg Front-Back", 0f), M("Left Lower Leg Stretch", 1f), M("Right Lower Leg Stretch", 1f), M("Left Arm Down-Up", -0.55f), M("Right Arm Down-Up", -0.55f), M("Head Nod Down-Up", 0f)),
            K(0.8f, M("Spine Front-Back", -0.3f), M("Left Upper Leg Front-Back", 0.3f), M("Right Upper Leg Front-Back", 0.1f), M("Left Lower Leg Stretch", 0.4f), M("Right Lower Leg Stretch", 0.7f), M("Left Arm Down-Up", 0.2f), M("Right Arm Down-Up", 0.3f), M("Head Nod Down-Up", -0.5f))
        });

        // Controller: nhân bản bộ Starter Assets (locomotion, nhảy) rồi thêm layer.
        AssetDatabase.CopyAsset(SourceController, ControllerPath);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("AttackIndex", AnimatorControllerParameterType.Int);
        controller.AddParameter("Hit", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Interact", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Roll", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

        AvatarMask upper = Mask("AM_An_UpperBody", AvatarMaskBodyPart.Body, AvatarMaskBodyPart.Head,
            AvatarMaskBodyPart.LeftArm, AvatarMaskBodyPart.RightArm, AvatarMaskBodyPart.LeftFingers, AvatarMaskBodyPart.RightFingers);
        AvatarMask leftArm = Mask("AM_An_LeftArm", AvatarMaskBodyPart.LeftArm, AvatarMaskBodyPart.LeftFingers);

        // Actions (nửa thân trên): chém 2 nhát, bị đánh, tương tác.
        AnimatorStateMachine actions = AddLayer(controller, "Actions", upper, 1f);
        AnimatorState emptyA = actions.AddState("Empty");
        actions.defaultState = emptyA;
        AnimatorState s1 = OneShot(actions, emptyA, slash1, "Slash1", 1f);
        AnyTrigger(actions, s1, "Attack", ("AttackIndex", 0));
        AnimatorState s2 = OneShot(actions, emptyA, slash2, "Slash2", 1f);
        AnyTrigger(actions, s2, "Attack", ("AttackIndex", 1));
        AnyTrigger(actions, OneShot(actions, emptyA, hit, "Hit", 1f), "Hit");
        AnyTrigger(actions, OneShot(actions, emptyA, interact, "Interact", 1f), "Interact");

        // FullBody: lộn và chết (thân được xoay thêm bằng PlayerAnimator qua transform Skeleton).
        AnimatorStateMachine full = AddLayer(controller, "FullBody", null, 1f);
        AnimatorState emptyF = full.AddState("Empty");
        full.defaultState = emptyF;
        AnyTrigger(full, OneShot(full, emptyF, roll, "Roll", 1f), "Roll");
        AnimatorState dead = full.AddState("Dead");
        dead.motion = death;
        AnimatorStateTransition toDead = full.AddAnyStateTransition(dead);
        toDead.AddCondition(AnimatorConditionMode.If, 0, "Dead");
        toDead.canTransitionToSelf = false;
        toDead.duration = 0.1f;
        AnimatorStateTransition revive = dead.AddTransition(emptyF);
        revive.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead");
        revive.duration = 0.1f;

        // LanternArm: tư thế giơ đèn tay trái; PlayerAnimator đặt trọng số theo đèn bật/tắt.
        AnimatorStateMachine lanternLayer = AddLayer(controller, "LanternArm", leftArm, 0f);
        AnimatorState hold = lanternLayer.AddState("LanternHold");
        hold.motion = lantern;
        lanternLayer.defaultState = hold;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
        Animator animator = root.GetComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        if (root.GetComponent<PlayerAnimator>() == null) root.AddComponent<PlayerAnimator>();
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log("AnAnimationBuilder: đã tạo AC_An và 7 clip, gán vào PF_An_Prototype.");
        MeleeVisualSetup.Rebuild();
    }

    private struct Key { public float time; public KeyValuePair<string, float>[] values; }

    private static KeyValuePair<string, float> M(string muscle, float value) => new KeyValuePair<string, float>(muscle, value);

    private static Key K(float time, params KeyValuePair<string, float>[] values) => new Key { time = time, values = values };

    private static Key K(float time, Dictionary<string, float> values)
    {
        var list = new List<KeyValuePair<string, float>>(values);
        return new Key { time = time, values = list.ToArray() };
    }

    // Mỗi muscle có mặt ở khóa nào thì có khóa đó; muscle vắng ở khóa giữa thì nội suy.
    private static AnimationClip Clip(string name, float length, bool loop, Key[] keys)
    {
        string path = $"{Folder}/{name}.anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        clip.ClearCurves();

        Dictionary<string, AnimationCurve> curves = new Dictionary<string, AnimationCurve>();
        foreach (Key key in keys)
            foreach (KeyValuePair<string, float> mv in key.values)
            {
                if (!curves.TryGetValue(mv.Key, out AnimationCurve curve)) curves[mv.Key] = curve = new AnimationCurve();
                curve.AddKey(new Keyframe(key.time, mv.Value));
            }
        foreach (KeyValuePair<string, AnimationCurve> pair in curves)
        {
            if (pair.Value.keys[pair.Value.length - 1].time < length)
                pair.Value.AddKey(new Keyframe(length, pair.Value.keys[pair.Value.length - 1].value));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), pair.Key), pair.Value);
        }

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static AvatarMask Mask(string name, params AvatarMaskBodyPart[] parts)
    {
        string path = $"{Folder}/{name}.mask";
        AvatarMask mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
        if (mask == null)
        {
            mask = new AvatarMask();
            AssetDatabase.CreateAsset(mask, path);
        }
        for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++) mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, false);
        foreach (AvatarMaskBodyPart part in parts) mask.SetHumanoidBodyPartActive(part, true);
        EditorUtility.SetDirty(mask);
        return mask;
    }

    private static AnimatorStateMachine AddLayer(AnimatorController controller, string name, AvatarMask mask, float weight)
    {
        AnimatorStateMachine sm = new AnimatorStateMachine { name = name, hideFlags = HideFlags.HideInHierarchy };
        AssetDatabase.AddObjectToAsset(sm, controller);
        controller.AddLayer(new AnimatorControllerLayer
        {
            name = name,
            stateMachine = sm,
            avatarMask = mask,
            defaultWeight = weight,
            blendingMode = AnimatorLayerBlendingMode.Override
        });
        return sm;
    }

    private static AnimatorState OneShot(AnimatorStateMachine sm, AnimatorState back, AnimationClip clip, string name, float speed)
    {
        AnimatorState state = sm.AddState(name);
        state.motion = clip;
        state.speed = speed;
        AnimatorStateTransition exit = state.AddTransition(back);
        exit.hasExitTime = true;
        exit.exitTime = 0.95f;
        exit.duration = 0.08f;
        return state;
    }

    private static void AnyTrigger(AnimatorStateMachine sm, AnimatorState target, string trigger, (string name, int value)? equals = null)
    {
        AnimatorStateTransition t = sm.AddAnyStateTransition(target);
        t.AddCondition(AnimatorConditionMode.If, 0, trigger);
        if (equals.HasValue) t.AddCondition(AnimatorConditionMode.Equals, equals.Value.value, equals.Value.name);
        t.duration = 0.06f;
        t.canTransitionToSelf = true;
    }
}
