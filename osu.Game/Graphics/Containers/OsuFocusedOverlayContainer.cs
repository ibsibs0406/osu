﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using OpenTK;
using osu.Framework.Configuration;
using osu.Framework.Input.Bindings;
using osu.Game.Audio;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Graphics.Containers
{
    public class OsuFocusedOverlayContainer : FocusedOverlayContainer, IPreviewTrackOwner, IKeyBindingHandler<GlobalAction>
    {
        private SampleChannel samplePopIn;
        private SampleChannel samplePopOut;

        protected virtual bool PlaySamplesOnStateChange => true;

        private PreviewTrackManager previewTrackManager;

        protected readonly Bindable<OverlayActivation> OverlayActivationMode = new Bindable<OverlayActivation>(OverlayActivation.All);

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));
            dependencies.CacheAs<IPreviewTrackOwner>(this);
            return dependencies;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame osuGame, AudioManager audio, PreviewTrackManager previewTrackManager)
        {
            this.previewTrackManager = previewTrackManager;

            if (osuGame != null)
                OverlayActivationMode.BindTo(osuGame.OverlayActivationMode);

            samplePopIn = audio.Sample.Get(@"UI/overlay-pop-in");
            samplePopOut = audio.Sample.Get(@"UI/overlay-pop-out");

            StateChanged += onStateChanged;
        }

        /// <summary>
        /// Whether mouse input should be blocked screen-wide while this overlay is visible.
        /// Performing mouse actions outside of the valid extents will hide the overlay.
        /// </summary>
        public virtual bool BlockScreenWideMouse => BlockPassThroughMouse;

        // receive input outside our bounds so we can trigger a close event on ourselves.
        public override bool ReceiveMouseInputAt(Vector2 screenSpacePos) => BlockScreenWideMouse || base.ReceiveMouseInputAt(screenSpacePos);

        protected override bool OnClick(InputState state)
        {
            if (!base.ReceiveMouseInputAt(state.Mouse.NativeState.Position))
            {
                State = Visibility.Hidden;
                return true;
            }

            return base.OnClick(state);
        }

        public bool OnPressed(GlobalAction action)
        {
            if (action == GlobalAction.Back)
            {
                State = Visibility.Hidden;
                return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action) => false;

        private void onStateChanged(Visibility visibility)
        {
            switch (visibility)
            {
                case Visibility.Visible:
                    if (OverlayActivationMode != OverlayActivation.Disabled)
                    {
                        if (PlaySamplesOnStateChange) samplePopIn?.Play();
                    }
                    else
                        State = Visibility.Hidden;
                    break;
                case Visibility.Hidden:
                    if (PlaySamplesOnStateChange) samplePopOut?.Play();
                    break;
            }
        }

        protected override void PopOut()
        {
            base.PopOut();
            previewTrackManager.StopAnyPlaying(this);
        }
    }
}
