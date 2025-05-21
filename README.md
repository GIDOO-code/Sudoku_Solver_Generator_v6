# Sudoku_Solver_Generator_v6.0
  Version6.0 2025.September will be released.

# GNPX v5 HP:
  en : https://gidoo-code.github.io/Sudoku_Solver_Generator_v6/<br>
  jp : https://gidoo-code.github.io/Sudoku_Solver_Generator_v6_jp/<br>
<br><br>

# Sudoku_Solver_Generator
![GNPX](./images0/GNPX_start.png)<br>


## 1. GNPX v5 brings significant improvements to the program.<br>
   GNPX v6 is a development and deployment version of new algorithms. Smartness is secondary.<br>
   There is no continuity with the traditional code in the analysis algorithm part of GNPX.<br>

## 2. The Sudoku analysis algorithm was studied.<br>
   i will explain using an image diagram to explain the logic.(Too specific, it will be difficult to understand the essence)t<br>
  (1) "Locked" in Sudoku analysis<br>
  (2) Extension of ALS (AnLS), extension of algorithm<br>
  (3) Link、network expansion<br>
  (4) Fish family<br>
  (5) SueDeCoq family, SueDeCoq's new algorithm( SueDeCoqEx, Franken SueDeCoq, Finned SueDeCoq )<br>
  (6) DeathBlossom algorithm consideration<br>
  (7) Subset<br>
  (8) Firework<br>
  (9) Exocet(junior/Senior) ... Senior Exocet is currently under development.<br>

## 3. GNPX v6 GNPXGNPX<br>
  (1) Classify and organize the processing contents using pages in the UI.<br>
  (2) Improve many analysis algorithms. All algorithms are expressed in bits (Bit81 changed to UInt128).<br>
  (3) Find solutions in the background and monitor the accuracy of the algorithm (using T/E method).<br>
  (4) Improve problem creation function (generate all Sudoku problems by specifying patterns and fixing block 1. Parallel processing. Approximately 9 million puzzles).<br>