﻿using System;
namespace QuantoAgent.Log {

    public class Message {
        public string Facility { get; set; }

        public int Level { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }

        public Message() {
            Name = "QuantoAgent";
        }

        public Message(string facility, int level, string text) {
            Facility = facility;
            Level = level;
            Text = text;
            Name = "QuantoAgent";
        }

        public Message(string facility, Level level, string text) {
            Facility = facility;
            Level = (int)level;
            Text = text;
            Name = "QuantoAgent";
        }

        public Message(string facility, int level, string name, string text) {
            Facility = facility;
            Level = level;
            Text = text;
            Name = name;
        }

        public Message(string facility, Level level, string name, string text) {
            Facility = facility;
            Level = (int)level;
            Text = text;
            Name = name;
        }
    }
}
