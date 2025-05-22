import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';
@Pipe({
  name: 'markdown'
})
export class MarkdownPipe implements PipeTransform {

  private cache = new Map<string, SafeHtml>();

  constructor(private sanitizer: DomSanitizer) {
    marked.setOptions({
      breaks: true,
      gfm: true
    });
  }
  transform(text: string): SafeHtml {
    if (!text) return '';

    const cacheKey = text + JSON.stringify({});
    if (this.cache.has(cacheKey)) {
      return this.cache.get(cacheKey);
    }

    const renderer = new marked.Renderer(); // Getting default renderer

    try {
      const htmlOutput = marked.parse(text, {
        renderer,
        async: false
      }) as string;

      const result = this.sanitizer.bypassSecurityTrustHtml(htmlOutput);

      // Manage cache size
      if (this.cache.size > 50) {
        const oldestKey = this.cache.keys().next().value;
        this.cache.delete(oldestKey);
      }

      this.cache.set(cacheKey, result);
      return result;
    } catch (error) {
      console.error('Error parsing markdown:', error);
      return this.sanitizer.bypassSecurityTrustHtml(text);
    }
  }
}
