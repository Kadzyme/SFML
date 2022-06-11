﻿using SFML.System;
using SFML.Window;

namespace Game
{
    public class PlayerSwapAbility
    {
        public Keyboard.Key key;

        private float normalCooldown = 2f;
        public float currentCooldown;

        public float Distance(Vector2f playerPos, Vector2f otherPlayerPos)
            => Math.Abs((otherPlayerPos.X - playerPos.X) + (otherPlayerPos.Y - playerPos.Y));

        public void SwapPlayers(Player playerToSwap1, Player playerToSwap2)
        {
            var playerShape = playerToSwap1.playerShape;
            playerToSwap1.playerShape = playerToSwap2.playerShape;
            playerToSwap2.playerShape = playerShape;
        }

        public void ResetCooldownOfAbility()
        {
            currentCooldown = normalCooldown;
        }
    }
}
