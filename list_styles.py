import sys, zipfile, re
sys.stdout.reconfigure(encoding='utf-8')
path = r'C:\Users\user\RiderProjects\Schedule\Диплом.docx'
with zipfile.ZipFile(path) as z:
    with z.open('word/styles.xml') as f:
        content = f.read().decode('utf-8')
# Find all style definitions
style_re = re.compile(r'<w:style[^>]+w:styleId="([^"]+)"[^>]*>.*?<w:name w:val="([^"]+)"', re.DOTALL)
for m in style_re.finditer(content):
    print(f'ID: {m.group(1):30s} Name: {m.group(2)}')
