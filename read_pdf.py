import sys
sys.stdout.reconfigure(encoding='utf-8')
import pdfplumber
with pdfplumber.open(r'C:\Users\user\Downloads\чет про диплом.pdf') as pdf:
    for i, page in enumerate(pdf.pages):
        text = page.extract_text()
        if text:
            print(f'=== PAGE {i+1} ===')
            print(text)
