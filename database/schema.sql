CREATE DATABASE IF NOT EXISTS warsim;
USE warsim;

CREATE TABLE IF NOT EXISTS simulation_runs (
  id INT AUTO_INCREMENT PRIMARY KEY,
  scenario_name VARCHAR(100) NOT NULL,
  seed INT NOT NULL,
  blue_strategy VARCHAR(30) NOT NULL,
  red_strategy VARCHAR(30) NOT NULL,
  winner VARCHAR(10) NOT NULL,
  turns_played INT NOT NULL,
  started_at DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS battle_events (
  id INT AUTO_INCREMENT PRIMARY KEY,
  run_id INT NOT NULL,
  turn INT NOT NULL,
  event_type VARCHAR(20) NOT NULL,
  unit_name VARCHAR(50) NOT NULL,
  target_name VARCHAR(50),
  damage INT,
  description VARCHAR(200),
  FOREIGN KEY (run_id) REFERENCES simulation_runs(id)
);

CREATE TABLE IF NOT EXISTS monte_carlo_batches (
  id INT AUTO_INCREMENT PRIMARY KEY,
  scenario_name VARCHAR(100) NOT NULL,
  runs INT NOT NULL,
  base_seed INT NOT NULL,
  blue_strategy VARCHAR(30) NOT NULL,
  red_strategy VARCHAR(30) NOT NULL,
  blue_wins INT NOT NULL,
  red_wins INT NOT NULL,
  draws INT NOT NULL,
  avg_turns DOUBLE NOT NULL,
  created_at DATETIME NOT NULL
);