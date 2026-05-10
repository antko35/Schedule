import sys, zipfile, re
sys.stdout.reconfigure(encoding='utf-8')
path = r'C:\Users\user\RiderProjects\Schedule\Диплом.docx'
with zipfile.ZipFile(path) as z:
    with z.open('word/document.xml') as f:
        content = f.read().decode('utf-8')

para_re = re.compile(r'<w:p[ >].*?</w:p>', re.DOTALL)
style_re = re.compile(r'<w:pStyle w:val="([^"]+)"')
text_re = re.compile(r'<w:t[^>]*>([^<]*)</w:t>')

results = []
for para in para_re.findall(content):
    style_m = style_re.search(para)
    style = style_m.group(1) if style_m else 'Normal'
    texts = text_re.findall(para)
    text = ''.join(texts).strip()
    if text:
        results.append(f'[{style}] {text}')

print('\n'.join(results))
