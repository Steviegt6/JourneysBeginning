﻿using Terraria.UI;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;
using System;

namespace JourneysBeginning.Content.UI {
    public abstract class BaseUI : UIState {
        /// <summary>
        /// Contains all things inheriting from <see cref="BaseUI"/>.
        /// </summary>
        public static List<BaseUI> UIs = new List<BaseUI>();

        internal bool visible;
        public string layerToSortAfter;

        protected string name;

        protected BaseUI _instance;
        private UserInterface _userInterface;

        protected BaseUI() : base() {
            Initializer();
            _userInterface = new UserInterface();
            _userInterface.SetState(_instance);
            UIs.Add(_instance);
        }
        /// <summary>
        /// For doing stuff related to your instance and fields. Called before <see cref="BaseUI()"/>'s list adding stuff.
        /// </summary>
        protected abstract void Initializer();

        public void UpdateUI(GameTime g) {
            if (visible)
                _userInterface?.Update(g);
        }
        public void SortAfter(List<GameInterfaceLayer> layers) {
            int index = layers.FindIndex(l => l.Name.Equals(layerToSortAfter));

            if (index != -1) {
                layers.Insert(index, new LegacyGameInterfaceLayer(JourneysBeginning.Instance.Name + ": " + name,
                    delegate {
                        if (visible) 
                            _userInterface.Draw(Terraria.Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
        public static void PerformOperationOnAllUI(Action<BaseUI> action) {
            foreach (BaseUI b in UIs)
                action(b);
        }
    }
}
