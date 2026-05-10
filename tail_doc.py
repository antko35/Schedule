import sys
sys.stdout.reconfigure(encoding='utf-8')
with open(r'C:\Users\user\RiderProjects\Schedule\diploma_unpacked\word\document.xml', encoding='utf-8') as f:
    lines = f.readlines()
total = len(lines)
print(f"Total lines: {total}")
for i, line in enumerate(lines[-40:], total-39):
    print(f"{i}: {line}", end='')
