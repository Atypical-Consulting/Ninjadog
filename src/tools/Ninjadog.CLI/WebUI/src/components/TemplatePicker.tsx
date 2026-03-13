import { useUiStore } from '../store/ui-store';
import { useConfigStore, type NinjadogState } from '../store/config-store';

const TEMPLATES: Record<string, { icon: string; title: string; desc: string; meta: string; state: NinjadogState }> = {
  blank: {
    icon: '\uD83D\uDCC4',
    title: 'Blank',
    desc: 'Start with an empty configuration file.',
    meta: '0 entities',
    state: { config: { name: '', version: '1.0.0', rootNamespace: '', outputPath: '.' }, entities: {} },
  },
  todo: {
    icon: '\u2705',
    title: 'Todo App',
    desc: 'A simple todo list API with categories.',
    meta: '2 entities',
    state: {
      config: { name: 'TodoApi', version: '1.0.0', rootNamespace: 'TodoApi', outputPath: '.', cors: { origins: ['http://localhost:3000'] } },
      entities: {
        TodoItem: {
          properties: {
            id: { type: 'Guid', isKey: true },
            title: { type: 'string', required: true, maxLength: 200 },
            description: { type: 'string', maxLength: 1000 },
            isCompleted: { type: 'bool' },
            createdAt: { type: 'DateTime' },
            dueDate: { type: 'DateTime' },
          },
        },
      },
    },
  },
  blog: {
    icon: '\uD83D\uDCDD',
    title: 'Blog API',
    desc: 'Posts, comments, and authors for a blog platform.',
    meta: '3 entities',
    state: {
      config: { name: 'BlogApi', version: '1.0.0', rootNamespace: 'BlogApi', outputPath: '.', features: { softDelete: true, auditing: true } },
      entities: {
        Post: {
          properties: { id: { type: 'Guid', isKey: true }, title: { type: 'string', required: true, maxLength: 200 }, content: { type: 'string' }, slug: { type: 'string', maxLength: 200 }, isPublished: { type: 'bool' } },
          relationships: { comments: { type: 'hasMany', targetEntity: 'Comment', foreignKey: 'postId' }, tags: { type: 'hasMany', targetEntity: 'Tag', foreignKey: 'postId' } },
        },
        Comment: {
          properties: { id: { type: 'Guid', isKey: true }, content: { type: 'string', required: true }, author: { type: 'string', maxLength: 100 } },
          relationships: { post: { type: 'belongsTo', targetEntity: 'Post', foreignKey: 'postId' } },
        },
        Tag: {
          properties: { id: { type: 'int', isKey: true }, name: { type: 'string', required: true, maxLength: 50 } },
        },
      },
    },
  },
  ecommerce: {
    icon: '\uD83D\uDED2',
    title: 'E-Commerce',
    desc: 'Products, orders, customers, and categories.',
    meta: '4 entities',
    state: {
      config: {
        name: 'ShopApi', version: '1.0.0', rootNamespace: 'ShopApi', outputPath: '.',
        database: { provider: 'postgres' }, features: { softDelete: true, auditing: true },
        cors: { origins: ['http://localhost:3000'], methods: ['GET', 'POST', 'PUT', 'DELETE'], headers: ['Content-Type', 'Authorization'] },
      },
      entities: {
        Product: { properties: { id: { type: 'Guid', isKey: true }, name: { type: 'string', required: true, maxLength: 200 }, description: { type: 'string' }, price: { type: 'decimal', required: true, min: 0 }, stock: { type: 'int', min: 0 }, sku: { type: 'string', maxLength: 50 } } },
        Customer: { properties: { id: { type: 'Guid', isKey: true }, email: { type: 'string', required: true, maxLength: 255 }, firstName: { type: 'string', required: true, maxLength: 100 }, lastName: { type: 'string', required: true, maxLength: 100 } }, relationships: { orders: { type: 'hasMany', targetEntity: 'Order', foreignKey: 'customerId' } } },
        Order: { properties: { id: { type: 'Guid', isKey: true }, orderDate: { type: 'DateTime' }, totalAmount: { type: 'decimal' }, status: { type: 'string', maxLength: 50 } }, relationships: { customer: { type: 'belongsTo', targetEntity: 'Customer', foreignKey: 'customerId' }, items: { type: 'hasMany', targetEntity: 'OrderItem', foreignKey: 'orderId' } } },
        OrderItem: { properties: { id: { type: 'Guid', isKey: true }, quantity: { type: 'int', required: true, min: 1 }, unitPrice: { type: 'decimal', required: true } }, relationships: { order: { type: 'belongsTo', targetEntity: 'Order', foreignKey: 'orderId' }, product: { type: 'belongsTo', targetEntity: 'Product', foreignKey: 'productId' } } },
      },
      enums: { OrderStatus: ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'] },
    },
  },
};

export default function TemplatePicker() {
  const open = useUiStore((s) => s.templatePickerOpen);
  const setOpen = useUiStore((s) => s.setTemplatePickerOpen);
  const showToast = useUiStore((s) => s.showToast);
  const pushUndo = useConfigStore((s) => s.pushUndo);
  const setState = useConfigStore((s) => s.setState);
  const onStateChanged = useConfigStore((s) => s.onStateChanged);

  const apply = (key: string) => {
    const template = TEMPLATES[key];
    if (!template) return;
    pushUndo();
    setState(JSON.parse(JSON.stringify(template.state)));
    onStateChanged();
    showToast('Template applied: ' + template.title, 'success');
    setOpen(false);
  };

  return (
    <div id="template-picker" className={`modal-overlay${!open ? ' hidden' : ''}`} onClick={(e) => e.target === e.currentTarget && setOpen(false)}>
      <div className="modal-panel template-picker-panel">
        <h2 className="template-picker-title">Start from a Template</h2>
        <p className="template-picker-subtitle">Choose a starter configuration or start from scratch.</p>
        <div id="template-grid" className="template-grid">
          {Object.entries(TEMPLATES).map(([key, t]) => (
            <div key={key} className="template-card" data-template={key} onClick={() => apply(key)}>
              <div className="template-card-icon">{t.icon}</div>
              <div className="template-card-title">{t.title}</div>
              <div className="template-card-desc">{t.desc}</div>
              <div className="template-card-meta">{t.meta}</div>
            </div>
          ))}
        </div>
        <div className="flex justify-end mt-4">
          <button id="template-picker-close" className="btn-sm btn-ghost" onClick={() => setOpen(false)}>Cancel</button>
        </div>
      </div>
    </div>
  );
}
