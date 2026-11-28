'use strict';
/* GeneratorJS — interface. Sem dependências: DOM puro, sem innerHTML com dados vindos do banco/XML. */

// ============================================================ utilidades
const $ = (s, r = document) => r.querySelector(s);
const wait = ms => new Promise(r => setTimeout(r, ms));

function h(tag, attrs, ...kids) {
    const el = document.createElement(tag);
    for (const [k, v] of Object.entries(attrs || {})) {
        if (v === null || v === undefined || v === false) continue;
        if (k === 'class') el.className = v;
        else if (k === 'text') el.textContent = v;
        else if (k.startsWith('on') && typeof v === 'function') el.addEventListener(k.slice(2), v);
        else if (v === true) el.setAttribute(k, '');
        else el.setAttribute(k, v);
    }
    const add = k => {
        if (Array.isArray(k)) k.forEach(add);
        else if (k instanceof Node) el.appendChild(k);
        else if (k !== null && k !== undefined && k !== false) el.appendChild(document.createTextNode(String(k)));
    };
    kids.forEach(add);
    return el;
}

// append/prepend/replaceChildren ignoram null/false/undefined e aceitam listas (evita o texto "null" na tela)
for (const m of ['append', 'prepend', 'replaceChildren']) {
    const orig = Element.prototype[m];
    Element.prototype[m] = function (...a) {
        return orig.apply(this, a.flat().filter(x => x !== null && x !== undefined && x !== false));
    };
}

const ICONS = {
    play: '<polygon points="6 4 20 12 6 20 6 4"/>',
    plug: '<path d="M9 2v6M15 2v6M7 8h10v4a5 5 0 0 1-10 0V8zM12 17v5"/>',
    shield: '<path d="M12 3 5 6v6c0 4.5 3 7.5 7 9 4-1.5 7-4.5 7-9V6z"/><path d="m9 12 2 2 4-4"/>',
    layout: '<rect x="3" y="3" width="18" height="18" rx="2"/><path d="M3 9h18M9 21V9"/>',
    history: '<path d="M3 12a9 9 0 1 0 3-6.7L3 8"/><path d="M3 3v5h5M12 7v5l3 2"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    trash: '<path d="M3 6h18M8 6V4h8v2M6 6l1 14h10l1-14"/>',
    edit: '<path d="M12 20h9M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/>',
    check: '<path d="M20 6 9 17l-5-5"/>',
    x: '<path d="M18 6 6 18M6 6l12 12"/>',
    folder: '<path d="M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/>',
    copy: '<rect x="9" y="9" width="12" height="12" rx="2"/><path d="M5 15V5a2 2 0 0 1 2-2h8"/>',
    refresh: '<path d="M21 12a9 9 0 1 1-3-6.7L21 8"/><path d="M21 3v5h-5"/>',
    db: '<ellipse cx="12" cy="5" rx="8" ry="3"/><path d="M4 5v6c0 1.7 3.6 3 8 3s8-1.3 8-3V5M4 11v6c0 1.7 3.6 3 8 3s8-1.3 8-3v-6"/>',
    file: '<path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8z"/><path d="M14 3v5h5"/>',
    alert: '<path d="M12 3 2 20h20z"/><path d="M12 10v4M12 17h.01"/>',
    info: '<circle cx="12" cy="12" r="9"/><path d="M12 11v5M12 8h.01"/>',
    eye: '<path d="M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12z"/><circle cx="12" cy="12" r="3"/>',
    stop: '<rect x="6" y="6" width="12" height="12" rx="1"/>',
    upload: '<path d="M12 15V3M7 8l5-5 5 5M5 21h14"/>',
    code: '<path d="m16 18 6-6-6-6M8 6l-6 6 6 6"/>',
    save: '<path d="M5 3h11l4 4v14H5z"/><path d="M8 3v6h8V3M8 21v-7h8v7"/>',
    star: '<path d="m12 2.5 2.9 6 6.6.9-4.8 4.6 1.2 6.5L12 17.4l-5.9 3.1 1.2-6.5L2.5 9.4l6.6-.9z"/>',
};

function icon(name, cls = '') {
    const s = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    s.setAttribute('viewBox', '0 0 24 24');
    s.setAttribute('class', ('i ' + cls).trim());
    s.setAttribute('aria-hidden', 'true');
    s.innerHTML = ICONS[name] || '';
    return s;
}

const esc = s => String(s).replace(/[&<>"]/g, c => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;'
}[c]));

function highlightJson(text) {
    const re = /("(?:\\.|[^"\\])*")(\s*:)?|\b(true|false)\b|\bnull\b|-?\d+(?:\.\d+)?(?:[eE][+-]?\d+)?/g;
    let out = '',
        last = 0,
        m;
    while ((m = re.exec(text))) {
        out += esc(text.slice(last, m.index));
        if (m[1] !== undefined) out += m[2] ? `<span class="tk-k">${esc(m[1])}</span>${esc(m[2])}` : `<span class="tk-s">${esc(m[1])}</span>`;
        else if (m[3]) out += `<span class="tk-b">${m[3]}</span>`;
        else if (m[0] === 'null') out += '<span class="tk-z">null</span>';
        else out += `<span class="tk-n">${m[0]}</span>`;
        last = re.lastIndex;
    }
    return out + esc(text.slice(last));
}

function highlightSql(text) {
    const re = /('(?:''|[^'])*')|(\b(?:SELECT|FROM|WHERE|ORDER BY|GROUP BY|AND|OR|TOP|LIMIT|FETCH FIRST|ROWS ONLY|FIRST|COUNT|LIKE|IS NOT NULL|IS NULL|AS)\b)|(@p\d+|:p\d+|\?)/gi;
    let out = '',
        last = 0,
        m;
    while ((m = re.exec(text))) {
        out += esc(text.slice(last, m.index));
        if (m[1]) out += `<span class="tk-s">${esc(m[1])}</span>`;
        else if (m[2]) out += `<span class="tk-k">${esc(m[2])}</span>`;
        else out += `<span class="tk-n">${esc(m[3])}</span>`;
        last = re.lastIndex;
    }
    return out + esc(text.slice(last));
}

function fmtMs(ms) {
    return ms < 1000 ? `${ms} ms` : `${(ms / 1000).toFixed(1)} s`;
}

function fmtBytes(b) {
    return b < 1024 ? `${b} B` : b < 1048576 ? `${(b / 1024).toFixed(1)} KB` : `${(b / 1048576).toFixed(1)} MB`;
}

function fmtDate(iso) {
    try {
        return new Date(iso).toLocaleString('pt-BR');
    } catch {
        return iso;
    }
}
const nf = n => Number(n).toLocaleString('pt-BR');

function toast(msg, kind = '') {
    const box = $('#toasts');
    // não empilha avisos idênticos (ex.: clicar várias vezes em Salvar)
    if ([...box.children].some(c => c.dataset.msg === msg)) return;
    const t = h('div', {
        'data-msg': msg,
        class: 'toast ' + kind
    }, icon(kind === 'err' ? 'alert' : kind === 'ok' ? 'check' : 'info'), h('div', {
        text: msg
    }));
    box.appendChild(t);
    // <dialog> modal fica na camada superior e cobre o resto da página: o contêiner de avisos
    // também vai para a camada superior (popover) para aparecer por cima do diálogo.
    try {
        if (box.showPopover) {
            if (!box.hasAttribute('popover')) box.setAttribute('popover', 'manual');
            if (box.matches(':popover-open')) box.hidePopover();
            box.showPopover();
        }
    } catch (e) { /* sem suporte: segue como estava */ }
    setTimeout(() => t.remove(), kind === 'err' ? 8000 : 4200);
}

function askConfirm(title, message, okLabel = 'Confirmar') {
    return new Promise(resolve => {
        const dlg = h('dialog', {
            style: 'width:min(440px,92vw)'
        },
            h('div', {
                class: 'dlg-head'
            }, h('h2', {
                text: title
            })),
            h('div', {
                class: 'dlg-body'
            }, h('p', {
                class: 'muted',
                text: message
            })),
            h('div', {
                class: 'dlg-foot'
            }, h('span', {
                class: 'spacer'
            }),
                h('button', {
                    class: 'btn',
                    text: 'Cancelar',
                    onclick: () => dlg.close('no')
                }),
                h('button', {
                    class: 'btn danger',
                    text: okLabel,
                    onclick: () => dlg.close('yes')
                })));
        dlg.addEventListener('close', () => {
            resolve(dlg.returnValue === 'yes');
            dlg.remove();
        });
        document.body.appendChild(dlg);
        dlg.showModal();
    });
}

// ============================================================ ponte com o C#
const listeners = {};
const on = (ev, fn) => (listeners[ev] = listeners[ev] || []).push(fn);
const emit = (ev, data) => (listeners[ev] || []).forEach(f => f(data));

function realBridge() {
    const wv = window.chrome.webview;
    let seq = 0;
    const pending = new Map();
    wv.addEventListener('message', e => {
        const m = e.data;
        if (m.event) return emit(m.event, m.data);
        const p = pending.get(m.id);
        if (!p) return;
        pending.delete(m.id);
        m.ok ? p.res(m.data) : p.rej(new Error(m.error));
    });
    return {
        call: (cmd, args = {}) => new Promise((res, rej) => {
            const id = ++seq;
            pending.set(id, {
                res,
                rej
            });
            wv.postMessage({
                id,
                cmd,
                args
            });
        })
    };
}

let bridge;
const call = (cmd, args) => bridge.call(cmd, args);

// ============================================================ estado
const prefs = (() => {
    let p = {};
    try {
        p = JSON.parse(localStorage.getItem('gjs.prefs') || '{}');
    } catch { }
    return new Proxy(p, {
        set(t, k, v) {
            t[k] = v;
            try {
                localStorage.setItem('gjs.prefs', JSON.stringify(t));
            } catch { }
            return true;
        }
    });
})();

const S = {
    view: 'gerar',
    info: {},
    providers: [],
    profiles: [],
    layouts: [],
    history: [],
    status: {}, // id da conexão -> {state, text}
    layoutId: null,
    layoutDetail: null,
    layoutTab: 'colunas',
};

const G = { // tela Gerar
    filters: [],
    tables: [],
    tab: 'consulta',
    preview: null,
    runId: null,
    running: false,
    logs: [],
    lastResult: null,
};

const VIEWS = {
    gerar: ['Gerar JSON', 'Leia a fonte de dados, aplique o layout XML e gere o arquivo do DOI.', 'play'],
    conexoes: ['Conexões', 'Fontes de dados cadastradas: bancos, CSV, TXT e Excel. As senhas ficam criptografadas neste computador.', 'plug'],
    layouts: ['Layouts', 'Mapeamentos XML que definem as colunas lidas e a estrutura do JSON.', 'layout'],
    historico: ['Histórico', 'Últimas execuções.', 'history'],
};

const PROV_ABBR = {
    sqlserver: 'SQL',
    postgres: 'PG',
    mysql: 'MY',
    oracle: 'ORA',
    firebird: 'FB',
    sqlite: 'LITE',
    access: 'ACC',
    csv: 'CSV',
    txt: 'TXT',
    excel: 'XLS'
};
const abbrOf = p => PROV_ABBR[p.key] || p.abbr || String(p.key || 'DB').slice(0, 3).toUpperCase();
const catOf = p => p.category || 'Outros';
const providerByKey = k => S.providers.find(p => p.key === k);
const provName = k => (providerByKey(k) || {}).name || k;
const profileById = id => S.profiles.find(p => p.id === id);
const layoutById = id => S.layouts.find(l => l.id === id);

// ============================================================ shell
function buildNav() {
    document.querySelectorAll('.nav').forEach(b => {
        const v = b.dataset.view;
        b.replaceChildren(icon(VIEWS[v][2]), h('span', {
            text: VIEWS[v][0]
        }),
            v === 'conexoes' && S.profiles.length ? h('span', {
                class: 'badge',
                text: String(S.profiles.length)
            }) : null,
            v === 'layouts' && S.layouts.length ? h('span', {
                class: 'badge',
                text: String(S.layouts.length)
            }) : null);
        b.onclick = () => go(v);
    });
}

function go(view) {
    S.view = view;
    document.querySelectorAll('.nav').forEach(b => b.classList.toggle('active', b.dataset.view === view));
    document.querySelectorAll('.view').forEach(v => v.classList.toggle('active', v.id === 'view-' + view));
    $('#title').textContent = VIEWS[view][0];
    $('#subtitle').textContent = VIEWS[view][1];
    const ta = $('#topActions');
    ta.replaceChildren();
    if (view === 'conexoes') ta.append(h('button', {
        class: 'btn primary',
        onclick: () => openConnDialog()
    }, icon('plus'), 'Nova conexão'));
    if (view === 'layouts') ta.append(
        h('button', {
            class: 'btn',
            onclick: reloadLayouts
        }, icon('refresh'), 'Recarregar'),
        h('button', {
            class: 'btn primary',
            onclick: importLayout
        }, icon('upload'), 'Importar XML'));
    if (view === 'historico') ta.append(h('button', {
        class: 'btn danger',
        onclick: clearHistory
    }, icon('trash'), 'Limpar'));
    if (view === 'historico') loadHistory();
}

function applyTheme(name) {
    document.documentElement.dataset.theme = name;
    prefs.theme = name;
    $('#theme').value = name;
    const cs = getComputedStyle(document.documentElement);
    const caption = cs.getPropertyValue('--header').trim();
    call('ui.theme', {
        dark: cs.getPropertyValue('--scheme').trim() === 'dark',
        caption
    }).catch(() => { });
}

// ============================================================ GERAR
function buildGerar() {
    const root = $('#view-gerar');
    G.el = {};
    const e = G.el;

    // A conexão é definida em "Conexões" (padrão). O select fica oculto só para guardar o valor
    // que o restante da tela já lê; o que o usuário vê é a caixa somente leitura (e.profileBox).
    e.profile = h('select', {
        id: 'selProfile',
        hidden: true,
        onchange: () => {
            prefs.profileId = e.profile.value;
            G.tables = [];
            refreshTableList();
            renderProfileBox();
            updateGerarMeta();
        }
    });
    e.profileBox = h('div', {
        class: 'conn-pick'
    });
    e.layout = h('select', {
        id: 'selLayout',
        onchange: () => {
            prefs.layoutId = e.layout.value;
            updateGerarMeta();
            refreshFilterColumns();
        }
    });
    e.table = h('input', {
        type: 'text',
        id: 'inpTable',
        list: 'dlTables',
        placeholder: 'Padrão do layout',
        onchange: () => {
            prefs.table = e.table.value;
        }
    });
    e.tableList = h('datalist', {
        id: 'dlTables'
    });
    e.btnTables = h('button', {
        class: 'btn icon',
        title: 'Carregar tabelas / planilhas da fonte',
        onclick: loadTables
    }, icon('refresh'));
    e.meta = h('div', {
        class: 'metaline'
    });
    e.banner = h('div');
    e.filters = h('div');
    e.folder = h('input', {
        type: 'text',
        placeholder: 'Pasta onde os arquivos serão gravados',
        onchange: () => {
            prefs.folder = e.folder.value;
        }
    });
    e.folderRow = h('div', {
        class: 'field'
    },
        h('label', {
            text: 'Pasta de destino'
        }),
        h('div', {
            class: 'row'
        }, h('div', {
            class: 'grow'
        }, e.folder), h('button', {
            class: 'btn',
            onclick: pickFolder
        }, icon('folder'), 'Escolher')));
    e.fileName = h('input', {
        type: 'text',
        onchange: () => {
            prefs.fileName = e.fileName.value;
        }
    });
    e.maxPer = h('input', {
        type: 'number',
        min: '0',
        placeholder: '0 = tudo em um arquivo',
        onchange: () => {
            prefs.maxPer = e.maxPer.value;
        }
    });
    e.emptyNull = h('input', {
        type: 'checkbox',
        onchange: () => {
            prefs.emptyNull = e.emptyNull.checked;
        }
    });
    e.fileFields = h('div', {},
        h('div', {
            class: 'grid2'
        },
            h('div', {
                class: 'field'
            }, h('label', {
                text: 'Nome do arquivo'
            }), e.fileName, h('span', {
                class: 'hint',
                text: 'Tokens: {layout} {date} {time}'
            })),
            h('div', {
                class: 'field'
            }, h('label', {
                text: 'Máx. registros por arquivo'
            }), e.maxPer)));
    e.modeSeg = h('div', {
        class: 'seg block'
    },
        h('button', {
            'data-m': 'files',
            onclick: () => setMode('files')
        }, icon('file', 'sm'), 'Arquivos'),
        h('button', {
            'data-m': 'virtual',
            onclick: () => setMode('virtual')
        }, icon('code', 'sm'), 'Virtual (memória)'));

    const left = h('div', {
        class: 'sticky'
    },
        h('div', {
            class: 'card'
        },
            h('h3', {}, h('span', {
                class: 'n',
                text: '1'
            }), 'Origem'),
            h('div', {
                class: 'field'
            }, h('label', {
                text: 'Conexão'
            }), e.profileBox, e.profile),
            h('div', {
                class: 'field'
            }, h('label', {
                text: 'Layout'
            }), e.layout),
            h('div', {
                class: 'field'
            }, h('label', {
                text: 'Tabela / planilha de origem'
            }),
                h('div', {
                    class: 'row'
                }, h('div', {
                    class: 'grow'
                }, e.table), e.btnTables), e.tableList,
                h('span', {
                    class: 'hint',
                    text: 'Em branco usa a tabela definida no layout.'
                })),
            e.meta),
        h('div', {
            class: 'card'
        },
            h('h3', {}, h('span', {
                class: 'n',
                text: '2'
            }), 'Filtros'),
            e.filters,
            h('button', {
                class: 'btn sm',
                onclick: () => {
                    G.filters.push({
                        column: '',
                        operator: 'eq',
                        value: ''
                    });
                    renderFilters();
                }
            }, icon('plus', 'sm'), 'Adicionar filtro')),
        h('div', {
            class: 'card'
        },
            h('h3', {}, h('span', {
                class: 'n',
                text: '3'
            }), 'Saída'),
            h('div', {
                class: 'field'
            }, e.modeSeg),
            e.folderRow, e.fileFields,
            h('label', {
                class: 'check',
                style: 'margin-top:12px'
            }, e.emptyNull,
                h('span', {}, 'Arrays sem filhos como null', h('small', {
                    text: 'Igual ao Extractor original. Desmarque para gravar [].'
                })))));

    // painel direito
    e.tabs = h('div', {
        class: 'tabs'
    });
    e.panel = h('div');
    const right = h('div', {}, e.banner, e.tabs, e.panel);

    e.status = h('span', {
        text: 'Pronto.'
    });
    e.spin = h('span', {
        class: 'spinner',
        style: 'display:none'
    });
    e.btnPreview = h('button', {
        class: 'btn',
        onclick: doPreview
    }, icon('eye'), 'Pré-visualizar');
    e.btnRun = h('button', {
        class: 'btn primary',
        onclick: doRun
    }, icon('play'), 'Gerar JSON');
    e.btnCancel = h('button', {
        class: 'btn danger',
        style: 'display:none',
        onclick: doCancel
    }, icon('stop'), 'Cancelar');
    const bar = h('div', {
        class: 'actionbar'
    }, h('div', {
        class: 'status'
    }, e.spin, e.status), e.btnPreview, e.btnCancel, e.btnRun);

    root.replaceChildren(h('div', {
        class: 'split'
    }, left, right), bar);

    e.fileName.value = prefs.fileName || '{layout}_{date}_{time}';
    e.maxPer.value = prefs.maxPer || '';
    e.folder.value = prefs.folder || S.info.defaultOutput || '';
    e.emptyNull.checked = prefs.emptyNull !== false;
    e.table.value = prefs.table || '';
    setMode(prefs.mode || 'files');
    renderGerarTabs();
}

function setMode(m) {
    prefs.mode = m;
    G.mode = m;
    G.el.modeSeg.querySelectorAll('button').forEach(b => b.classList.toggle('on', b.dataset.m === m));
    G.el.folderRow.style.display = m === 'files' ? '' : 'none';
    G.el.fileFields.style.display = m === 'files' ? '' : 'none';
}

function refreshGerarSelects() {
    const e = G.el;
    if (!e.profile) return;
    e.profile.replaceChildren(
        h('option', {
            value: '',
            text: S.profiles.length ? 'Selecione uma conexão…' : 'Nenhuma conexão cadastrada'
        }),
        ...S.profiles.map(p => h('option', {
            value: p.id,
            text: `${p.name} — ${provName(p.providerKey)}`
        })));
    e.layout.replaceChildren(
        h('option', {
            value: '',
            text: S.layouts.length ? 'Selecione um layout…' : 'Nenhum layout encontrado'
        }),
        ...S.layouts.map(l => h('option', {
            value: l.id,
            text: `${l.name} (${l.id})`
        })));
    // quem define a conexão é a tela Conexões: padrão (flag) ou, se houver só uma, ela mesma
    const padrao = S.profiles.find(p => p.isDefault);
    if (padrao) e.profile.value = padrao.id;
    else if (S.profiles.length === 1) e.profile.value = S.profiles[0].id;
    else e.profile.value = '';
    renderProfileBox();
    if (layoutById(prefs.layoutId)) e.layout.value = prefs.layoutId;
    else if (S.layouts.length === 1) e.layout.value = S.layouts[0].id;
    updateGerarMeta();
    renderFilters();
}

function renderProfileBox() {
    const box = G.el.profileBox;
    if (!box) return;
    const p = profileById(G.el.profile.value);
    const irParaConexoes = txt => h('button', {
        class: 'btn sm',
        onclick: () => go('conexoes')
    }, icon('plug', 'sm'), txt);
    box.classList.toggle('warn', !p);
    if (p) {
        const prov = providerByKey(p.providerKey) || { key: p.providerKey };
        box.replaceChildren(
            h('div', { class: 'pv sm', text: abbrOf(prov) }),
            h('div', { class: 'txt' },
                h('b', { text: p.name, title: p.name }),
                h('div', { class: 'sub', text: `${provName(p.providerKey)} · ${p.summary || '—'}`, title: p.summary })),
            irParaConexoes('Alterar'));
        return;
    }
    box.replaceChildren(
        h('div', { class: 'txt' },
            h('b', { text: S.profiles.length ? 'Nenhuma conexão padrão' : 'Nenhuma conexão cadastrada' }),
            h('div', {
                class: 'sub',
                text: S.profiles.length
                    ? 'Marque a estrela (★) de uma conexão em Conexões.'
                    : 'Cadastre uma fonte de dados em Conexões.'
            })),
        irParaConexoes(S.profiles.length ? 'Escolher' : 'Cadastrar'));
}

function currentLayoutSummary() {
    return layoutById(G.el.layout.value);
}

function updateGerarMeta() {
    const e = G.el,
        l = currentLayoutSummary(),
        p = profileById(e.profile.value);
    e.meta.replaceChildren();
    e.banner.replaceChildren();
    if (l) {
        e.table.placeholder = l.table || 'Padrão do layout';
        e.meta.append(
            h('span', {
                class: 'chip'
            }, icon('db', 'sm'), l.table),
            h('span', {
                class: 'chip',
                text: `${l.columns} colunas`
            }),
            l.errors ? h('span', {
                class: 'badge err',
                text: `${l.errors} erro(s)`
            }) : null,
            l.warnings ? h('span', {
                class: 'badge warn',
                text: `${l.warnings} aviso(s)`
            }) : null);
        if (l.errors) e.banner.append(h('div', {
            class: 'banner err'
        }, icon('alert'),
            h('div', {}, h('b', {
                text: 'Este layout tem erros de validação.'
            }), 'A geração fica bloqueada até corrigir o XML. Veja os detalhes em Layouts → Validação.')));
        else if (l.warnings) e.banner.append(h('div', {
            class: 'banner warn'
        }, icon('alert'),
            h('div', {}, h('b', {
                text: `${l.warnings} aviso(s) no layout.`
            }), 'A geração é permitida, mas vale conferir em Layouts → Validação (ex.: campos com valor fixo parecido com nome de coluna).')));
    }
    if (p) e.meta.append(h('span', {
        class: 'chip'
    }, icon('plug', 'sm'), provName(p.providerKey)));
}

function refreshTableList() {
    G.el.tableList.replaceChildren(...G.tables.map(t => h('option', {
        value: t
    })));
}

async function loadTables() {
    const pid = G.el.profile.value;
    if (!pid) return toast('Selecione uma conexão primeiro.', 'err');
    setBusy(true, 'Lendo tabelas…');
    try {
        G.tables = await call('schema.tables', {
            profileId: pid
        });
        refreshTableList();
        toast(`${G.tables.length} item(ns) carregado(s). Digite no campo para filtrar.`, 'ok');
        G.el.table.focus();
    } catch (err) {
        toast(err.message, 'err');
    }
    setBusy(false);
}

async function pickFolder() {
    const p = await call('dialog.folder', {
        path: G.el.folder.value
    });
    if (p) {
        G.el.folder.value = p;
        prefs.folder = p;
    }
}

// ---- filtros
const OPS = [
    ['eq', '='],
    ['ne', '≠'],
    ['gt', '>'],
    ['ge', '≥'],
    ['lt', '<'],
    ['le', '≤'],
    ['contains', 'contém'],
    ['startswith', 'começa com'],
    ['isnull', 'é nulo'],
    ['notnull', 'não é nulo']
];

function layoutColumnNames() {
    const d = S.layoutDetail && S.layoutDetail.id === G.el.layout.value ? S.layoutDetail : null;
    return d ? d.columns.map(c => c.name) : [];
}

async function refreshFilterColumns() {
    const id = G.el.layout.value;
    if (id && (!S.layoutDetail || S.layoutDetail.id !== id)) {
        try {
            S.layoutDetail = await call('layouts.get', {
                id
            });
        } catch { }
    }
    renderFilters();
}

function renderFilters() {
    const cols = layoutColumnNames();
    const box = G.el.filters;
    box.replaceChildren();
    if (!G.filters.length) box.append(h('p', {
        class: 'hint',
        style: 'margin:0 0 10px',
        text: 'Sem filtros: todos os registros da tabela entram no JSON.'
    }));
    G.filters.forEach((f, i) => {
        const col = h('select', {
            onchange: () => {
                f.column = col.value;
            }
        },
            h('option', {
                value: '',
                text: 'Coluna…'
            }), ...cols.map(c => h('option', {
                value: c,
                text: c
            })));
        col.value = f.column;
        const op = h('select', {
            onchange: () => {
                f.operator = op.value;
                val.disabled = ['isnull', 'notnull'].includes(f.operator);
            }
        },
            ...OPS.map(([v, t]) => h('option', {
                value: v,
                text: t
            })));
        op.value = f.operator;
        const val = h('input', {
            type: 'text',
            placeholder: 'valor',
            value: f.value,
            oninput: () => {
                f.value = val.value;
            }
        });
        val.disabled = ['isnull', 'notnull'].includes(f.operator);
        box.append(h('div', {
            class: 'filter-row'
        }, col, op, val,
            h('button', {
                class: 'btn ghost icon',
                title: 'Remover',
                onclick: () => {
                    G.filters.splice(i, 1);
                    renderFilters();
                }
            }, icon('x', 'sm'))));
    });
}

// ---- abas
function renderGerarTabs() {
    const e = G.el;
    const tabs = [
        ['consulta', 'Consulta', 'db'],
        ['previa', 'Prévia', 'eye'],
        ['json', 'JSON', 'code'],
        ['execucao', 'Execução', 'play']
    ];
    e.tabs.replaceChildren(...tabs.map(([k, t, ic]) =>
        h('button', {
            class: 'tab' + (G.tab === k ? ' on' : ''),
            onclick: () => {
                G.tab = k;
                renderGerarTabs();
            }
        }, icon(ic, 'sm'), t)));
    const p = e.panel;
    p.replaceChildren();

    if (G.tab === 'consulta') {
        if (!G.preview) return p.append(emptyBox('db', 'Veja a consulta antes de executar', 'Escolha conexão e layout e clique em “Pré-visualizar”. Para bancos, o SQL é montado no dialeto do provider, com filtros sempre parametrizados.'));
        const pr = G.preview;
        p.append(
            h('div', {
                class: 'toolbar'
            }, h('span', {
                class: 'muted',
                text: 'SQL gerado'
            }), h('span', {
                class: 'spacer'
            }),
                h('button', {
                    class: 'btn sm',
                    onclick: () => copy(pr.sql)
                }, icon('copy', 'sm'), 'Copiar')),
            codeBlock(highlightSql(pr.sql), true),
            pr.parameters.length ? h('p', {
                class: 'muted mono',
                style: 'font-size:12px',
                text: 'Parâmetros: ' + pr.parameters.join('  ·  ')
            }) : null,
            ...pr.warnings.map(w => h('div', {
                class: 'banner warn'
            }, icon('alert'), h('div', {
                text: w
            }))));
    }

    if (G.tab === 'previa') {
        if (!G.preview) return p.append(emptyBox('eye', 'Nenhuma prévia ainda', 'A prévia lê só as primeiras 5 linhas e mostra os valores já convertidos pelo layout.'));
        const pr = G.preview;
        p.append(
            h('div', {
                class: 'toolbar'
            },
                h('span', {
                    class: 'chip ok'
                }, h('span', {
                    class: 'dot'
                }), pr.totalRows != null ? `${nf(pr.totalRows)} registro(s) no total` : 'total desconhecido'),
                h('span', {
                    class: 'muted',
                    text: `mostrando ${pr.rows.length}`
                })),
            pr.rows.length ? h('div', {
                class: 'table-wrap'
            }, h('table', {
                class: 't'
            },
                h('thead', {}, h('tr', {}, ...pr.columns.map(c => h('th', {
                    text: c
                })))),
                h('tbody', {}, ...pr.rows.map(r => h('tr', {}, ...r.map(v => v === null ? h('td', {
                    class: 'null',
                    text: 'null'
                }) : h('td', {
                    title: v,
                    text: v
                }))))))) :
                emptyBox('file', 'Nenhum registro', 'Os filtros atuais não retornaram linhas.'));
    }

    if (G.tab === 'json') {
        const json = G.lastResult && G.lastResult.json ? G.lastResult.json : G.preview ? G.preview.json : null;
        const src = G.lastResult && G.lastResult.json ? 'JSON virtual gerado' : 'Prévia do JSON (primeiras linhas)';
        if (!json) return p.append(emptyBox('code', 'Sem JSON para mostrar', 'Use “Pré-visualizar”, ou gere em modo Virtual para ver o resultado completo aqui.'));
        const big = json.length > 300000;
        p.append(
            h('div', {
                class: 'toolbar'
            }, h('span', {
                class: 'muted',
                text: src + (big ? ' — exibindo os primeiros 300 KB' : '')
            }), h('span', {
                class: 'spacer'
            }),
                h('button', {
                    class: 'btn sm',
                    onclick: () => copy(json)
                }, icon('copy', 'sm'), 'Copiar'),
                h('button', {
                    class: 'btn sm',
                    onclick: () => saveJson(json)
                }, icon('save', 'sm'), 'Salvar como…')),
            codeBlock(highlightJson(big ? json.slice(0, 300000) : json)));
    }

    if (G.tab === 'execucao') renderRun(p);
}

function emptyBox(ic, title, text) {
    return h('div', {
        class: 'empty card'
    }, icon(ic, 'lg'), h('h2', {
        text: title
    }), h('p', {
        text
    }));
}

function codeBlock(html, wrap) {
    const pre = h('pre', {
        class: 'code' + (wrap ? ' wrap' : '')
    });
    pre.innerHTML = html; // html já escapado por highlightJson/highlightSql
    return pre;
}

async function copy(text) {
    try {
        await navigator.clipboard.writeText(text);
        toast('Copiado.', 'ok');
    } catch {
        toast('Não foi possível copiar.', 'err');
    }
}

async function saveJson(content) {
    try {
        const p = await call('file.save', {
            name: 'saida.json',
            content
        });
        if (p) toast('Arquivo salvo: ' + p, 'ok');
    } catch (err) {
        toast(err.message, 'err');
    }
}

// ---- execução
function renderRun(p) {
    const r = G.lastResult;
    p.append(
        h('div', {
            class: 'progress' + (G.running ? '' : ' done'),
            id: 'pbar'
        }, h('i', {
            style: `width:${G.pct || 0}%`
        })),
        h('div', {
            style: 'height:12px'
        }));
    if (r) p.append(resultSummary(r), h('div', {
        style: 'height:6px'
    }));
    const log = h('div', {
        class: 'log',
        id: 'log'
    });
    G.logEl = log;
    G.logs.forEach(l => log.append(logLine(l)));
    if (!G.logs.length) log.append(h('div', {
        class: 'info'
    }, h('span', {
        text: 'Aguardando execução…'
    })));
    p.append(log);
    log.scrollTop = log.scrollHeight;
}

function logLine(l) {
    return h('div', {
        class: l.level
    }, h('time', {
        text: new Date(l.time).toLocaleTimeString('pt-BR')
    }), h('span', {
        text: l.message
    }));
}

function resultSummary(r) {
    const wrap = h('div');
    wrap.append(h('div', {
        class: 'summary'
    },
        h('div', {
            class: 'stat ' + (r.success ? 'ok' : 'bad')
        }, h('small', {
            text: 'Resultado'
        }), h('b', {
            text: r.success ? 'Concluído' : r.cancelled ? 'Cancelado' : 'Falhou'
        })),
        h('div', {
            class: 'stat'
        }, h('small', {
            text: 'Registros'
        }), h('b', {
            text: nf(r.rows)
        })),
        h('div', {
            class: 'stat'
        }, h('small', {
            text: 'Tempo'
        }), h('b', {
            text: fmtMs(r.elapsedMs)
        })),
        r.issueCount ? h('div', {
            class: 'stat bad'
        }, h('small', {
            text: 'Conversões com falha'
        }), h('b', {
            text: nf(r.issueCount)
        })) : null));
    if (r.error) wrap.append(h('div', {
        class: 'banner err'
    }, icon('alert'), h('div', {
        text: r.error
    })));
    if (r.files && r.files.length) wrap.append(h('div', {
        class: 'files'
    }, ...r.files.map(f =>
        h('div', {
            class: 'file'
        }, icon('file'), h('span', {
            class: 'path',
            title: f.path
        }, h('bdi', {
            text: f.path
        })),
            h('small', {
                text: `${nf(f.rows)} reg. · ${fmtBytes(f.bytes)}`
            }),
            h('button', {
                class: 'btn sm',
                onclick: () => call('shell.open', {
                    path: f.path
                }).catch(e => toast(e.message, 'err'))
            }, icon('folder', 'sm'), 'Mostrar')))));
    const empties = Object.entries(r.emptyByColumn || {}).sort((a, b) => b[1] - a[1]);
    if (r.success && empties.length) wrap.append(h('div', {
        class: 'banner warn'
    }, icon('info'),
        h('div', {}, h('b', {
            text: 'Colunas com valores vazios'
        }),
            empties.slice(0, 8).map(([c, n]) => `${c}: ${nf(n)}`).join('  ·  ') + (empties.length > 8 ? `  ·  +${empties.length - 8}` : ''))));
    if (r.issues && r.issues.length) wrap.append(h('div', {
        class: 'banner err'
    }, icon('alert'),
        h('div', {}, h('b', {
            text: `Exemplos de valores que viraram null (${nf(r.issueCount)} no total)`
        }), r.issues.slice(0, 5).join(' | '))));
    return wrap;
}

function setBusy(busy, text) {
    const e = G.el;
    e.spin.style.display = busy ? '' : 'none';
    e.status.textContent = text || (busy ? 'Processando…' : 'Pronto.');
    e.btnPreview.disabled = busy;
    e.btnRun.disabled = busy;
}

function buildRequest() {
    const e = G.el;
    if (!e.profile.value) {
        toast('Selecione uma conexão.', 'err');
        return null;
    }
    if (!e.layout.value) {
        toast('Selecione um layout.', 'err');
        return null;
    }
    return {
        profileId: e.profile.value,
        layoutId: e.layout.value,
        table: e.table.value.trim() || null,
        filters: G.filters.filter(f => f.column).map(f => ({
            column: f.column,
            operator: f.operator,
            value: f.value
        })),
        mode: G.mode,
        outputFolder: e.folder.value.trim() || null,
        fileName: e.fileName.value.trim() || '{layout}_{date}_{time}',
        maxPerFile: parseInt(e.maxPer.value || '0', 10) || 0,
        emptyArrayAsNull: e.emptyNull.checked,
    };
}

async function doPreview() {
    const req = buildRequest();
    if (!req) return;
    setBusy(true, 'Conectando e lendo a prévia…');
    try {
        G.preview = await call('generate.preview', {
            request: req
        });
        G.lastResult = null;
        G.tab = 'previa';
        renderGerarTabs();
        toast('Prévia carregada.', 'ok');
    } catch (err) {
        toast(err.message, 'err');
        G.tab = 'consulta';
        renderGerarTabs();
    }
    setBusy(false);
}

async function doRun() {
    const req = buildRequest();
    if (!req) return;
    if (req.mode === 'files' && !req.outputFolder) return toast('Escolha a pasta de destino.', 'err');
    G.logs = [];
    G.pct = 0;
    G.lastResult = null;
    G.running = true;
    G.tab = 'execucao';
    renderGerarTabs();
    setBusy(true, 'Gerando…');
    G.el.btnRun.style.display = 'none';
    G.el.btnPreview.style.display = 'none';
    G.el.btnCancel.style.display = '';
    try {
        const r = await call('generate.start', {
            request: req
        });
        G.runId = r.runId;
    } catch (err) {
        finishRun(null, err.message);
    }
}

async function doCancel() {
    if (G.runId) {
        G.el.status.textContent = 'Cancelando…';
        await call('generate.cancel', {
            runId: G.runId
        });
    }
}

function finishRun(result, errorMsg) {
    G.running = false;
    G.runId = null;
    G.lastResult = result || {
        success: false,
        error: errorMsg,
        rows: 0,
        elapsedMs: 0,
        files: []
    };
    G.el.btnRun.style.display = '';
    G.el.btnPreview.style.display = '';
    G.el.btnCancel.style.display = 'none';
    setBusy(false, G.lastResult.success ? `Concluído: ${nf(G.lastResult.rows)} registro(s).` : 'Execução finalizada com problemas.');
    if (G.lastResult.success) {
        toast(G.lastResult.files && G.lastResult.files.length ? `JSON gerado (${G.lastResult.files.length} arquivo[s]).` : 'JSON virtual gerado.', 'ok');
        if (G.mode === 'virtual' && G.lastResult.json) G.tab = 'json';
        else G.tab = 'execucao';
    } else toast(G.lastResult.error || 'A execução falhou.', 'err');
    G.pct = G.lastResult.success ? 100 : G.pct;
    renderGerarTabs();
    loadHistory();
}

on('log', d => {
    if (d.runId !== G.runId) return;
    G.logs.push(d.entry);
    if (d.entry.percent != null) G.pct = d.entry.percent;
    if (G.tab === 'execucao' && G.logEl) {
        G.logEl.append(logLine(d.entry));
        G.logEl.scrollTop = G.logEl.scrollHeight;
        const bar = $('#pbar > i');
        if (bar) bar.style.width = (G.pct || 0) + '%';
    }
    G.el.status.textContent = d.entry.message.length > 90 ? d.entry.message.slice(0, 90) + '…' : d.entry.message;
});
on('done', d => {
    if (d.runId === G.runId) finishRun(d.result);
});

// ============================================================ CONEXÕES
function renderConexoes() {
    const root = $('#view-conexoes');
    if (!S.profiles.length) {
        root.replaceChildren(h('div', {
            class: 'empty card'
        }, icon('plug', 'lg'), h('h2', {
            text: 'Nenhuma conexão ainda'
        }),
            h('p', {
                text: 'Cadastre uma fonte de dados (banco, CSV, TXT ou Excel) para o gerador ler os registros.'
            }),
            h('button', {
                class: 'btn primary',
                onclick: () => openConnDialog()
            }, icon('plus'), 'Nova conexão')));
        return;
    }
    root.replaceChildren(h('div', {
        class: 'cards'
    }, ...S.profiles.map(p => {
        const st = S.status[p.id];
        const prov = providerByKey(p.providerKey) || {
            key: p.providerKey
        };
        return h('div', {
            class: 'conn'
        },
            h('div', {
                class: 'head'
            },
                h('div', {
                    class: 'pv',
                    text: abbrOf(prov)
                }),
                h('div', {
                    style: 'min-width:0;flex:1'
                }, h('h4', {
                    text: p.name,
                    title: p.name
                }), h('div', {
                    class: 'sub',
                    text: `${provName(p.providerKey)} · ${p.summary || '—'}`,
                    title: p.summary
                })),
                h('button', {
                    class: 'star-btn' + (p.isDefault ? ' on' : ''),
                    title: p.isDefault ? 'Conexão padrão (clique para remover)' : 'Usar como conexão padrão em Gerar JSON',
                    'aria-pressed': p.isDefault ? 'true' : 'false',
                    onclick: () => setDefaultProfile(p)
                }, icon('star'))),
            st ? h('span', {
                class: 'chip ' + st.state,
                title: st.state === 'err' ? st.text + '\n(clique para expandir/recolher)' : null,
                onclick: st.state === 'err' ? (e) => e.currentTarget.classList.toggle('open') : null
            }, h('span', {
                class: 'dot'
            }), h('span', {
                class: 'ctext',
                text: st.text
            })) : h('span', {
                class: 'chip'
            }, h('span', {
                class: 'dot'
            }), 'Não testada'),
            h('div', {
                class: 'foot'
            },
                h('button', {
                    class: 'btn sm',
                    disabled: prov.enabled === false,
                    title: prov.enabled === false ? 'Provider desativado' : null,
                    onclick: () => testSaved(p)
                }, icon('refresh', 'sm'), 'Testar'),
                h('button', {
                    class: 'btn sm',
                    onclick: () => openConnDialog(p)
                }, icon('edit', 'sm'), 'Editar'),
                h('span', {
                    class: 'spacer'
                }),
                h('button', {
                    class: 'btn sm ghost danger',
                    title: 'Excluir',
                    onclick: () => delProfile(p)
                }, icon('trash', 'sm'))));
    })));
}

async function testSaved(p) {
    S.status[p.id] = {
        state: 'busy',
        text: 'Testando…'
    };
    renderConexoes();
    try {
        const r = await call('profiles.test', {
            profile: {
                id: p.id,
                name: p.name,
                providerKey: p.providerKey,
                mode: p.mode,
                fields: p.fields,
                rawConnectionString: null
            }
        });
        S.status[p.id] = r.ok ? {
            state: 'ok',
            text: `Conectado em ${fmtMs(r.elapsedMs)}${r.serverVersion ? ' · v' + r.serverVersion : ''}`
        } : {
            state: 'err',
            text: r.message
        };
    } catch (err) {
        S.status[p.id] = {
            state: 'err',
            text: err.message
        };
    }
    renderConexoes();
}

async function setDefaultProfile(p) {
    const tornar = !p.isDefault;
    await call('profiles.setDefault', {
        id: tornar ? p.id : ''
    });
    await loadProfiles();
    if (tornar && G.el.profile) {   // a tela Gerar já passa a usar a conexão escolhida
        G.el.profile.value = p.id;
        G.el.profile.onchange();
    }
    toast(tornar ? `“${p.name}” é a conexão padrão.` : 'Conexão padrão removida.', 'ok');
}

async function delProfile(p) {
    if (!(await askConfirm('Excluir conexão', `Excluir “${p.name}”? Esta ação não pode ser desfeita.`, 'Excluir'))) return;
    await call('profiles.delete', {
        id: p.id
    });
    await loadProfiles();
    toast('Conexão excluída.', 'ok');
}

// ---- diálogo de conexão: o PROVIDER define os campos (como o [ProviderSpecific] do DatabasePropertyConfiguration)
let D = null; // estado do diálogo

async function openConnDialog(existing) {
    D = existing ?
        {
            id: existing.id,
            name: existing.name,
            providerKey: existing.providerKey,
            mode: existing.mode,
            fields: {
                ...existing.fields
            },
            raw: '',
            hasSecret: existing.hasSecret,
            hasRaw: existing.hasRaw,
            test: null
        } :
        {
            id: crypto.randomUUID().replace(/-/g, ''),
            name: '',
            providerKey: S.providers[0].key,
            mode: 'fields',
            fields: {},
            raw: '',
            hasSecret: false,
            hasRaw: false,
            test: null
        };
    if (!existing) applyDefaults();
    // modo "string completa": traz a string salva (descriptografada pelo app) para aparecer ao editar
    if (existing && existing.mode === 'raw' && existing.hasRaw) {
        try {
            D.raw = (await call('profiles.getRaw', { id: existing.id })) || '';
        } catch (e) {
            D.raw = '';
        }
    }
    renderConnDialog();
    $('#dlgConn').showModal();
}

function applyDefaults() {
    const p = providerByKey(D.providerKey);
    D.fields = {};
    p.fields.forEach(f => {
        if (f.default != null) D.fields[f.id] = f.default;
    });
}

function switchProvider(key) {
    D.providerKey = key;
    D.mode = 'fields';
    D.raw = '';
    D.test = null;
    D.hasSecret = false;
    D.hasRaw = false;
    applyDefaults();
    renderConnDialog();
}

function renderConnDialog() {
    const dlg = $('#dlgConn');
    const prov = providerByKey(D.providerKey);
    const off = prov.enabled === false;   // provider desativado: sem string completa, teste ou salvar
    const cat = catOf(prov);
    const cats = [...new Set(S.providers.map(catOf))];
    const canRaw = prov.supportsRaw !== false;
    const nameInp = h('input', {
        type: 'text',
        value: D.name,
        placeholder: 'Ex.: Cartório — produção',
        'data-role': 'connName',
        oninput: () => {
            D.name = nameInp.value;
            nameInp.classList.remove('invalid');
            setFormError(null);
        }
    });

    // 1) categoria (Banco de dados / Arquivo texto / Planilha...) — só aparece se houver mais de uma
    const catSeg = cats.length > 1 ?
        h('div', {
            class: 'seg block',
            style: 'margin-bottom:10px'
        }, ...cats.map(c =>
            h('button', {
                type: 'button',
                class: c === cat ? 'on' : '',
                text: c,
                onclick: () => {
                    if (c !== cat) switchProvider(S.providers.find(p => catOf(p) === c).key);
                }
            }))) :
        null;

    // 2) provider dentro da categoria
    const provBtns = h('div', {
        class: 'providers'
    }, ...S.providers.filter(p => catOf(p) === cat).map(p =>
        h('button', {
            class: 'pbtn' + (p.key === D.providerKey ? ' on' : '') + (p.enabled === false ? ' off' : ''),
            type: 'button',
            disabled: p.enabled === false,
            title: p.enabled === false ? 'Desativado' : null,
            onclick: () => {
                if (p.key !== D.providerKey) switchProvider(p.key);
            }
        }, h('b', {
            text: abbrOf(p)
        }), h('span', {
            text: p.name
        }))));

    // 3) campos do provider (ou string completa, quando o provider aceita)
    let rawSwitch = null;
    if (canRaw) rawSwitch = h('input', {
        type: 'checkbox',
        checked: D.mode === 'raw',
        disabled: off,
        onchange: async () => {
            const toRaw = rawSwitch.checked;
            rawSwitch.disabled = true;
            try {
                // converte o preenchimento atual para o outro formato antes de trocar a tela
                if (toRaw) await convertFieldsToRaw();
                else await convertRawToFields();
                D.mode = toRaw ? 'raw' : 'fields';
                D.test = null;
                renderConnDialog();
            } catch (err) {
                rawSwitch.checked = !toRaw; // não conseguiu converter: continua no modo atual
                rawSwitch.disabled = false;
                toast(err.message, 'err');
            }
        }
    });

    let body;
    if (canRaw && D.mode === 'raw') {
        const ta = h('textarea', {
            placeholder: D.hasRaw ? '•••••• (string salva — digite para substituir ou desmarque a opção para ver os campos)' : 'Server=...;Database=...;User Id=...;Password=...',
            oninput: () => {
                D.raw = ta.value;
            }
        });
        ta.value = D.raw;
        body = h('div', {
            class: 'field'
        }, h('label', {
            text: 'String de conexão'
        }), ta, h('span', {
            class: 'hint',
            text: 'Fica criptografada neste computador ao salvar.'
        }));
    } else {
        body = renderFields(prov);
    }

    D.testBox = h('div');
    paintTest();
    D.errBox = h('div', {
        class: 'dlg-error'
    });
    paintFormError();

    dlg.replaceChildren(
        h('div', {
            class: 'dlg-head'
        }, h('h2', {
            text: S.profiles.some(p => p.id === D.id) ? 'Editar conexão' : 'Nova conexão'
        }),
            h('button', {
                class: 'btn ghost icon',
                type: 'button',
                onclick: () => dlg.close()
            }, icon('x'))),
        h('div', {
            class: 'dlg-body'
        },
            h('div', {
                class: 'field'
            }, h('label', {
                text: 'Nome'
            }), nameInp),
            h('div', {
                class: 'lbl',
                style: 'margin-bottom:8px',
                text: 'Tipo de origem'
            }), catSeg,
            h('div', {
                class: 'lbl',
                style: 'margin-bottom:8px',
                text: 'Provider'
            }), provBtns,
            body,
            rawSwitch ? h('label', {
                class: 'check',
                style: 'margin-top:6px' + (off ? ';opacity:.45;cursor:not-allowed' : '')
            }, rawSwitch,
                h('span', {}, 'Usar string de conexão completa', h('small', {
                    text: 'Para cenários avançados. Marcar/desmarcar converte os campos e a string automaticamente.'
                }))) : null,
            off ? h('div', {
                class: 'hint warnline',
                style: 'margin-top:8px',
                text: 'Este provider está desativado: não é possível usar string completa, testar nem salvar.'
            }) : null,
            D.testBox),
        D.errBox,   // erro de validação/salvar: fica fixo acima do rodapé, sempre visível
        h('div', {
            class: 'dlg-foot'
        },
            h('button', {
                class: 'btn',
                type: 'button',
                disabled: off,
                onclick: testDraft
            }, icon('refresh'), prov.testLabel || 'Testar conexão'),
            h('span', {
                class: 'spacer'
            }),
            h('button', {
                class: 'btn',
                type: 'button',
                onclick: () => dlg.close()
            }, 'Cancelar'),
            h('button', {
                class: 'btn primary',
                type: 'button',
                disabled: off,
                onclick: saveDraft
            }, icon('check'), 'Salvar')));
}

// Agrupa os campos do provider (group nulo = principal; "Avançado" começa recolhido)
// Campos -> string de conexão. Se o formulário ainda está vazio, mantém o que já havia.
async function convertFieldsToRaw() {
    const raw = await call('profiles.toRaw', {
        profile: {
            ...draftProfile(),
            mode: 'fields',
            rawConnectionString: null
        }
    });
    if (raw) {
        D.raw = raw;
        D.hasRaw = false;
    }
}

// String de conexão -> campos. Preserva o que não faz parte da string (ex.: timeout de comando).
async function convertRawToFields() {
    const raw = (D.raw || '').trim();
    if (!raw && !D.hasRaw) return; // nada para converter
    const parsed = await call('profiles.fromRaw', {
        providerKey: D.providerKey,
        profileId: D.id,
        raw
    });
    const keep = D.fields.commandTimeout ? {
        commandTimeout: D.fields.commandTimeout
    } : {};
    applyDefaults(); // volta aos padrões do provider e aplica por cima o que veio da string
    Object.assign(D.fields, keep, parsed);
    D.raw = '';
    D.hasRaw = false;
}

function renderFields(prov) {
    const groups = new Map();
    prov.fields.forEach(f => {
        const g = f.group || '';
        if (!groups.has(g)) groups.set(g, []);
        groups.get(g).push(f);
    });
    const out = [];
    for (const [g, fs] of groups) {
        const items = fs.map(connField);
        if (!g) out.push(...items);
        else out.push(h('details', {
            class: 'grp',
            open: g !== 'Avançado'
        }, h('summary', {
            text: g
        }), h('div', {}, items)));
    }
    return h('div', {}, out);
}

function fieldWrap(f, ...controls) {
    return h('div', {
        class: 'field'
    },
        h('label', {}, f.label, f.required ? h('span', {
            class: 'hint',
            text: 'obrigatório'
        }) : null),
        ...controls, f.hint ? h('span', {
            class: 'hint',
            text: f.hint
        }) : null);
}

function connField(f) {
    if (f.kind === 'bool') {
        const cb = h('input', {
            type: 'checkbox',
            checked: (D.fields[f.id] ?? f.default) === 'true',
            onchange: () => {
                D.fields[f.id] = String(cb.checked);
            }
        });
        return h('div', {
            class: 'field'
        }, h('label', {
            class: 'check'
        }, cb, h('span', {
            text: f.label
        })));
    }
    if (f.kind === 'tribool') { // IsBooleanType: Undefined (não grava a chave) / True / False
        const sel = h('select', {
            onchange: () => {
                if (sel.value) D.fields[f.id] = sel.value;
                else delete D.fields[f.id];
            }
        },
            h('option', {
                value: '',
                text: 'Padrão do driver'
            }), h('option', {
                value: 'true',
                text: 'Sim'
            }), h('option', {
                value: 'false',
                text: 'Não'
            }));
        sel.value = D.fields[f.id] ?? '';
        return fieldWrap(f, sel);
    }
    if (f.kind === 'enum') {
        const opts = f.options || [];
        const sel = h('select', {
            onchange: () => {
                D.fields[f.id] = sel.value;
            }
        },
            ...opts.map(o => h('option', {
                value: o.value,
                text: o.label
            })));
        sel.value = D.fields[f.id] ?? f.default ?? (opts[0] ? opts[0].value : '');
        if (D.fields[f.id] === undefined && sel.value) D.fields[f.id] = sel.value; // grava o valor exibido
        return fieldWrap(f, sel);
    }
    if (f.kind === 'file' || f.kind === 'folder') {
        const inp = h('input', {
            type: 'text',
            placeholder: f.placeholder || '',
            autocomplete: 'off',
            oninput: () => {
                D.fields[f.id] = inp.value;
            }
        });
        inp.value = D.fields[f.id] ?? '';
        const pick = h('button', {
            class: 'btn',
            type: 'button',
            onclick: async () => {
                try {
                    const p = await call(f.kind === 'folder' ? 'dialog.folder' : 'dialog.file', {
                        path: inp.value,
                        filter: f.filter
                    });
                    if (p) {
                        inp.value = p;
                        D.fields[f.id] = p;
                    }
                } catch (err) {
                    toast(err.message, 'err');
                }
            }
        }, icon('folder'), 'Escolher');
        return fieldWrap(f, h('div', {
            class: 'row'
        }, h('div', {
            class: 'grow'
        }, inp), pick));
    }
    if (f.kind === 'combo') return comboField(f);

    const type = f.kind === 'password' ? 'password' : f.kind === 'number' ? 'number' : 'text';
    const inp = h('input', {
        type,
        placeholder: f.secret && D.hasSecret ? '•••••• (mantida — digite para trocar)' : (f.placeholder || ''),
        autocomplete: 'off',
        oninput: () => {
            D.fields[f.id] = inp.value;
        }
    });
    inp.value = D.fields[f.id] ?? '';
    return fieldWrap(f, inp);
}

// Campo livre + sugestões clicáveis (ex.: Provider OLE DB, aba do Excel). O valor digitado nunca é limitado à lista.
function comboField(f) {
    const opts = f.options || [];
    const status = h('span', {
        class: 'hint'
    });
    const chips = h('div', {
        class: 'opt-chips'
    });
    const inp = h('input', {
        type: 'text',
        placeholder: f.placeholder || '',
        autocomplete: 'off',
        spellcheck: 'false'
    });
    const paint = () => {
        const v = (D.fields[f.id] || '').trim();
        const o = opts.find(x => x.value.toLowerCase() === v.toLowerCase());
        status.textContent = !v ? 'Automático: usa o melhor provider instalado.' :
            o ? (o.installed ? 'Provider instalado neste aplicativo.' : 'Provider não encontrado neste aplicativo.') :
                'Provider personalizado (não verificado).';
        status.classList.toggle('warnline', !!v && !!o && !o.installed);
        chips.replaceChildren(...opts.map(x => h('button', {
            type: 'button',
            class: 'opt-chip ' + (x.installed ? 'ok' : 'miss') + (x.value.toLowerCase() === v.toLowerCase() ? ' on' : ''),
            title: x.label,
            onclick: () => {
                D.fields[f.id] = x.value;
                inp.value = x.value;
                paint();
            }
        }, x.value)));
    };
    inp.value = D.fields[f.id] ?? '';
    inp.addEventListener('input', () => {
        D.fields[f.id] = inp.value;
        paint();
    });
    paint();
    return h('div', {
        class: 'field'
    }, h('label', {
        text: f.label
    }), inp, chips, status, f.hint ? h('span', {
        class: 'hint',
        text: f.hint
    }) : null);
}

function draftProfile() {
    return {
        id: D.id,
        name: D.name.trim(),
        providerKey: D.providerKey,
        mode: D.mode,
        fields: D.fields,
        rawConnectionString: D.raw || null
    };
}

function paintTest() {
    const t = D.test;
    if (!t) return D.testBox.replaceChildren();
    if (t.busy) return D.testBox.replaceChildren(h('div', {
        class: 'testbox'
    }, h('span', {
        class: 'spinner'
    }), 'Testando conexão…'));
    D.testBox.replaceChildren(h('div', {
        class: 'testbox ' + (t.ok ? 'ok' : 'bad')
    }, icon(t.ok ? 'check' : 'alert'),
        h('div', {}, h('b', {
            text: t.ok ? 'Conexão estabelecida' : 'Falha na conexão'
        }),
            h('div', {
                text: t.ok ? `${fmtMs(t.elapsedMs)}${t.serverVersion ? ' · servidor ' + t.serverVersion : ''}` : t.message
            }))));
}

async function testDraft() {
    if (providerByKey(D.providerKey)?.enabled === false) return;
    setFormError(null);
    D.test = {
        busy: true
    };
    paintTest();
    try {
        D.test = await call('profiles.test', {
            profile: draftProfile()
        });
    } catch (err) {
        D.test = {
            ok: false,
            message: err.message
        };
    }
    paintTest();
}

// Erro mostrado dentro do diálogo (o toast da página fica atrás do fundo escuro do <dialog>)
function setFormError(msg) {
    D.formError = msg || null;
    paintFormError();
}

function paintFormError() {
    if (!D || !D.errBox) return;
    if (!D.formError) return D.errBox.replaceChildren();
    D.errBox.replaceChildren(h('div', {
        class: 'testbox bad',
        role: 'alert'
    }, icon('alert'), h('div', {}, h('b', {
        text: 'Não foi possível salvar'
    }), h('div', {
        text: D.formError
    }))));
}

// Leva o usuário ao campo com problema (o diálogo rola; o nome fica no topo)
function focusConnName() {
    const inp = $('#dlgConn input[data-role="connName"]');
    if (!inp) return;
    inp.classList.add('invalid');
    inp.scrollIntoView({ block: 'center', behavior: 'smooth' });
    inp.focus({ preventScroll: true });
}

async function saveDraft() {
    if (providerByKey(D.providerKey)?.enabled === false) return;
    if (!D.name.trim()) {
        setFormError('Informe um nome para a conexão.');
        focusConnName();
        return;
    }
    setFormError(null);
    try {
        await call('profiles.save', {
            profile: draftProfile()
        });
        $('#dlgConn').close();
        await loadProfiles();
        toast('Conexão salva.', 'ok');
    } catch (err) {
        setFormError(err.message);
    }
}

// ============================================================ LAYOUTS
async function renderLayouts() {
    const root = $('#view-layouts');
    const lic = licenseCard();
    if (!S.layouts.length) {
        const semLicenca = !S.license || (!S.license.valid && !S.license.bypass);
        root.replaceChildren(lic, h('div', {
            class: 'empty card'
        }, icon(semLicenca ? 'shield' : 'layout', 'lg'), h('h2', {
            text: semLicenca ? 'Nenhum modelo liberado' : 'Nenhum layout encontrado'
        }),
            h('p', {
                text: semLicenca
                    ? 'Importe a licença do cartório (.lic) enviada pelo fornecedor para liberar os modelos contratados.'
                    : 'Coloque um XML na pasta Layouts ou use “Importar XML”.'
            })));
        return;
    }
    if (!S.layoutId || !layoutById(S.layoutId)) S.layoutId = S.layouts[0].id;
    if (!S.layoutDetail || S.layoutDetail.id !== S.layoutId) {
        try {
            S.layoutDetail = await call('layouts.get', {
                id: S.layoutId
            });
        } catch (err) {
            toast(err.message, 'err');
        }
    }
    const d = S.layoutDetail;
    const list = h('div', {
        class: 'lay-list'
    }, ...S.layouts.map(l =>
        h('button', {
            class: 'lay-item' + (l.id === S.layoutId ? ' on' : ''),
            onclick: () => {
                S.layoutId = l.id;
                S.layoutDetail = null;
                renderLayouts();
            }
        },
            h('b', {
                text: l.name
            }), h('small', {
                text: `${l.id} · ${l.source}`
            }),
            h('div', {
                class: 'badges'
            },
                h('span', {
                    class: 'badge',
                    text: `${l.columns} colunas`
                }),
                l.errors ? h('span', {
                    class: 'badge err',
                    text: `${l.errors} erro(s)`
                }) : h('span', {
                    class: 'badge ok',
                    text: 'válido'
                }),
                l.warnings ? h('span', {
                    class: 'badge warn',
                    text: `${l.warnings} aviso(s)`
                }) : null))));

    const tabs = [
        ['colunas', 'Colunas'],
        ['json', 'Estrutura JSON'],
        ['validacao', 'Validação'],
        ['xml', 'XML']
    ];
    const tabBar = h('div', {
        class: 'tabs'
    }, ...tabs.map(([k, t]) =>
        h('button', {
            class: 'tab' + (S.layoutTab === k ? ' on' : ''),
            onclick: () => {
                S.layoutTab = k;
                renderLayouts();
            }
        }, t,
            k === 'validacao' && d && d.findings.length ? h('span', {
                class: 'badge ' + (d.findings.some(f => f.level === 'error') ? 'err' : d.findings.some(f => f.level === 'warning') ? 'warn' : ''),
                text: String(d.findings.length)
            }) : null)));

    const panel = h('div');
    if (d) {
        if (S.layoutTab === 'colunas') panel.append(h('div', {
            class: 'table-wrap'
        }, h('table', {
            class: 't'
        },
            h('thead', {}, h('tr', {}, ...['#', 'Coluna', 'Tipo', 'Formato', 'Descrição', 'Ordena'].map(x => h('th', {
                text: x
            })))),
            h('tbody', {}, ...d.columns.map((c, i) => h('tr', {},
                h('td', {
                    class: 'mono muted',
                    text: String(i + 1)
                }), h('td', {
                    class: 'mono',
                    text: c.name
                }), h('td', {
                    text: c.type
                }),
                h('td', {
                    class: 'mono',
                    text: c.format || '—'
                }), h('td', {
                    text: c.description || '—'
                }), h('td', {
                    text: c.orderBy ? 'sim' : ''
                })))))));
        if (S.layoutTab === 'json') panel.append(h('div', {
            class: 'card'
        }, d.tree ? renderTree(d) : h('p', {
            class: 'muted',
            text: 'Sem estrutura JSON.'
        })),
            h('p', {
                class: 'hint'
            }, 'Verde = lido da fonte (Columnrefe) · amarelo = valor fixo · vermelho = valor fixo parecido com nome de coluna (provável erro).'));
        if (S.layoutTab === 'validacao') panel.append(...(d.findings.length ? d.findings.map(f =>
            h('div', {
                class: 'find ' + f.level
            }, icon(f.level === 'info' ? 'info' : 'alert'), h('div', {}, h('code', {
                text: f.code
            }), f.message))) : [h('div', {
                class: 'banner'
            }, icon('check'), h('div', {
                text: 'Nenhum problema encontrado.'
            }))]));
        if (S.layoutTab === 'xml') panel.append(h('div', {
            class: 'toolbar'
        }, h('span', {
            class: 'muted mono',
            text: d.file
        }), h('span', {
            class: 'spacer'
        }),
            h('button', {
                class: 'btn sm',
                onclick: () => copy(d.xml)
            }, icon('copy', 'sm'), 'Copiar')),
            (() => {
                const pre = h('pre', {
                    class: 'code'
                });
                pre.textContent = d.xml.replace(/^\uFEFF/, '');
                return pre;
            })());
    }
    root.replaceChildren(lic, h('div', {
        class: 'lay'
    }, list, h('div', {}, d ? h('div', {
        class: 'row',
        style: 'margin-bottom:12px'
    },
        h('div', {}, h('h2', {
            style: 'margin:0;font-size:17px',
            text: d.name
        }), h('span', {
            class: 'muted mono',
            text: `tabela ${d.table} (${d.alias}) → ${d.rootName || '?'}[]`
        }))) : null, tabBar, panel)));
}

function renderTree(d) {
    const sus = new Set(d.findings.filter(f => f.code === 'VALOR_PARECE_COLUNA').map(f => f.path));
    const walk = (n, path) => {
        const here = path ? path + ' → ' + n.name : n.name;
        let tail;
        if (n.columnRef) tail = h('span', {
            class: 'bind',
            text: `⟵ ${n.columnRef}${n.format ? '  [' + n.format + ']' : ''}`
        });
        else if (n.value !== null && n.value !== undefined && !n.children.length) tail = h('span', {
            class: sus.has(here) ? 'sus' : 'fix',
            text: `= "${n.value}"${sus.has(here) ? '  ⚠ é nome de coluna?' : ''}`
        });
        else if (!n.children.length && n.type !== 'array') tail = h('span', {
            class: 'nul',
            text: 'sem origem → null'
        });
        else if (!n.children.length) tail = h('span', {
            class: 'nul',
            text: '[ ] vazio'
        });
        const li = h('li', {}, h('span', {
            class: 'nm',
            text: n.name
        }), h('span', {
            class: 'ty',
            text: n.type
        }), tail);
        if (n.children.length) li.append(h('ul', {}, ...n.children.map(c => walk(c, here))));
        return li;
    };
    return h('div', {
        class: 'tree'
    }, h('ul', {}, walk(d.tree, '')));
}

/** Sem licença válida a tela inteira fica inerte (nada clicável/editável) e só resta importar o .lic ou pedir uma licença. */
function applyLicenseLock() {
    const L = S.license;
    const bloqueado = !L || !L.valid;
    const app = document.querySelector('.app');
    if (app) app.inert = bloqueado;
    let el = document.getElementById('lockScreen');
    if (!bloqueado) {
        if (el) el.remove();
        return;
    }
    if (!el) {
        el = h('div', { id: 'lockScreen', class: 'lock', role: 'alertdialog', 'aria-modal': 'true' });
        document.body.appendChild(el);
    }
    lockMostrar(el, 'inicio');
}

// Contato do suporte (WhatsApp). O QR abre a conversa com uma mensagem pronta.
const CONTATO = { exibicao: '(31) 99720-0734', numero: '31997200734' };
const CONTATO_QR = [
    '11111110000110001010110100100001101111111',
    '10000010011110001010100000010100001000001',
    '10111010101000001000111000100000001011101',
    '10111010111100000010000001111100001011101',
    '10111010110001000001100111100011001011101',
    '10000010111111001010110001111100001000001',
    '11111110101010101010101010101010101111111',
    '00000000111100010010111000110000100000000',
    '10111110001000110110100001010101101111100',
    '10111000110001111001011110100011111110101',
    '01001011110101011110000000011000101010010',
    '01100001100010000000010010111001101111000',
    '01010010110010011100000001010001010101100',
    '00110001110010000010101000001010111111111',
    '11100010111101111011001010010000010011110',
    '10011001011010010001110110000000001001001',
    '11000010000110001111000111111100110101101',
    '01000001011111001101000100000011001111001',
    '01110010011110000100101000111000000011010',
    '11001000111000001111010110011001010010010',
    '01110111000010110000010011110100010001100',
    '11000100011000001011100110011011101110011',
    '01010110011110011100010000111010100010100',
    '11111000001111001001011110111001100110010',
    '01111010111001100110100001010111010001100',
    '10101001011110001101011110101011011111011',
    '10110110100110001010010000111110100010010',
    '01000100010001101001010110111001100011000',
    '01010111000110001100101011001111110101110',
    '11100000101111101000011001101101001111011',
    '10101111111101001100111010111000011110000',
    '10011000010101100000011110011001011111001',
    '10000010000100001100111011110100111110110',
    '00000000110001010011000101000111100010001',
    '11111110001110001010101001111101101010110',
    '10000010101110110001000010110010100010001',
    '10111010110101101010111111110100111111110',
    '10111010110011111001110000100111010100011',
    '10111010101101010110001010111101011110000',
    '10000010000000001100011110100010001011010',
    '11111110110110011000100001010101000110100'
];

function qrSvg(rows, px) {
    const q = 4, n = rows.length, t = n + q * 2;
    let d = '';
    rows.forEach((linha, y) => {
        let x = 0;
        while (x < n) {
            if (linha[x] === '1') {
                let w = 1;
                while (x + w < n && linha[x + w] === '1') w++;
                d += `M${x + q} ${y + q}h${w}v1h-${w}z`;
                x += w;
            } else x++;
        }
    });
    const wrap = h('div', { class: 'qr', title: 'QR code do WhatsApp do suporte' });
    wrap.innerHTML = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${t} ${t}" width="${px}" height="${px}" shape-rendering="crispEdges" role="img" aria-label="QR code do WhatsApp do suporte"><rect width="${t}" height="${t}" fill="#fff"/><path d="${d}" fill="#000"/></svg>`;
    return wrap;
}

async function copiarTexto(texto) {
    try {
        await navigator.clipboard.writeText(texto);
    } catch (e) {
        const t = document.createElement('textarea');
        t.value = texto;
        document.body.appendChild(t);
        t.select();
        document.execCommand('copy');
        t.remove();
    }
}

function contatoBloco(comQr) {
    return h('div', { class: 'lock-contato' },
        comQr ? qrSvg(CONTATO_QR, 112) : null,
        h('div', { class: 'lock-contato-txt' },
            h('b', { text: 'Suporte pelo WhatsApp' }),
            h('span', { class: 'mono', text: CONTATO.exibicao }),
            h('button', {
                class: 'btn sm',
                onclick: async () => { await copiarTexto(CONTATO.numero); toast('Número copiado.', 'ok'); }
            }, 'Copiar número'),
            comQr ? h('small', { text: 'Escaneie o QR code ou salve o número e envie o código acima.' }) : null));
}

const lockDados = { nome: '', cns: '', responsavel: '', email: '' };

function lockMostrar(el, tela, codigo) {
    const L = S.license;

    if (tela === 'solicitar') {
        const campos = {};
        let gerar;
        // Já existe licença (vencida, de outro computador…)? Os dados dela entram e ficam bloqueados.
        // Sem licença, tudo fica liberado para digitar. Campo que a licença não tem continua editável.
        const atual = L && L.nome ? L : null;
        if (atual) {
            const r = atual.responsavel || {};
            if (atual.nome) lockDados.nome = atual.nome;
            if (atual.cns) lockDados.cns = atual.cns;
            if (r.nome) lockDados.responsavel = r.nome;
            if (r.email) lockDados.email = r.email;
        }
        const daLicenca = {
            nome: atual && atual.nome, cns: atual && atual.cns,
            responsavel: atual && atual.responsavel && atual.responsavel.nome,
            email: atual && atual.responsavel && atual.responsavel.email
        };
        const campo = (id, rotulo) => {
            const inp = h('input', { type: 'text' });
            inp.value = lockDados[id] || '';
            if (daLicenca[id]) {
                inp.readOnly = true;
                inp.title = 'Dado da licença atual';
            }
            inp.oninput = () => { lockDados[id] = inp.value; };
            inp.onkeydown = e => { if (e.key === 'Enter' && gerar) gerar.click(); };
            campos[id] = inp;
            return h('div', { class: 'field' }, h('label', { text: rotulo }), inp);
        };
        gerar = h('button', {
            class: 'btn primary',
            onclick: async () => {
                try {
                    const r = await call('license.request', {
                        nome: lockDados.nome, cns: lockDados.cns,
                        responsavel: lockDados.responsavel, email: lockDados.email
                    });
                    lockMostrar(el, 'codigo', r.code);
                } catch (err) {
                    toast(err.message, 'err');
                }
            }
        }, icon('shield', 'sm'), 'Gerar código');
        el.replaceChildren(h('div', { class: 'lock-card lock-form' },
            h('h2', { text: 'Solicitar licença' }),
            h('p', {
                text: atual
                    ? 'Os dados abaixo vêm da licença atual e não podem ser alterados. O código gerado identifica este computador e deve ser enviado ao fornecedor.'
                    : 'Informe os dados do cartório. O código gerado identifica este computador e deve ser enviado ao fornecedor.'
            }),
            campo('nome', 'Nome do cartório *'),
            h('div', { class: 'lock-grid' }, campo('cns', 'CNS'), campo('responsavel', 'Responsável')),
            campo('email', 'E-mail ou telefone de contato'),
            h('div', { class: 'lock-actions' },
                h('button', { class: 'btn', onclick: () => lockMostrar(el, 'inicio') }, 'Voltar'), gerar)));
        (daLicenca.nome ? campos.email : campos.nome).focus();
        return;
    }

    if (tela === 'codigo') {
        const saida = h('textarea', { rows: 5, class: 'mono' });
        saida.readOnly = true;
        saida.value = codigo;
        el.replaceChildren(h('div', { class: 'lock-card lock-form' },
            h('h2', { text: 'Código de solicitação' }),
            h('p', { text: 'Copie o código e envie ao fornecedor pelo WhatsApp. Quando receber o arquivo .lic, volte aqui e use “Importar licença”.' }),
            saida,
            h('div', { class: 'lock-actions' },
                h('button', {
                    class: 'btn primary',
                    onclick: async () => {
                        saida.select();
                        try { await navigator.clipboard.writeText(saida.value); } catch (e) { document.execCommand('copy'); }
                        toast('Código copiado.', 'ok');
                    }
                }, icon('save', 'sm'), 'Copiar código')),
            h('div', { class: 'lock-actions' },
                h('button', { class: 'btn sm', onclick: () => lockMostrar(el, 'solicitar') }, 'Editar dados'),
                h('button', { class: 'btn sm', onclick: () => lockMostrar(el, 'inicio') }, 'Já tenho a licença')),
            contatoBloco(true)));
        saida.focus();
        saida.select();
        return;
    }

    el.replaceChildren(h('div', { class: 'lock-card' },
        h('div', { class: 'lock-ico' }, icon('shield', 'lg')),
        h('h2', { text: 'Licença do cartório necessária' }),
        h('p', { text: (L && L.message) || 'Nenhuma licença do cartório importada.' }),
        h('p', { class: 'muted', text: 'Importe o arquivo .lic enviado pelo fornecedor ou gere um código de solicitação para pedir uma nova licença. Até lá, nenhuma tela pode ser usada ou editada.' }),
        h('div', { class: 'lock-actions' },
            h('button', { class: 'btn primary', onclick: importLicense }, icon('save', 'sm'), 'Importar licença'),
            h('button', { class: 'btn', onclick: () => lockMostrar(el, 'solicitar') }, icon('shield', 'sm'), 'Gerar código de solicitação')),
        contatoBloco(false)));
}

function licenseCard() {
    const L = S.license;
    if (!L) return h('div');
    const ok = L.valid;
    const estado = L.bypass ? ['badge warn', 'modo desenvolvimento'] : ok ? ['badge ok', 'licença válida'] : ['badge err', 'sem licença válida'];
    const info = [];
    if (L.nome) info.push(h('b', { text: L.nome }));
    const linha = [L.cns && `CNS ${L.cns}`, L.codigoIbge && `IBGE ${L.codigoIbge}`].filter(Boolean).join(' · ');
    if (linha) info.push(h('div', { class: 'sub', text: linha }));
    if (L.responsavel && L.responsavel.nome)
        info.push(h('div', { class: 'sub', text: `Responsável: ${L.responsavel.nome}${L.responsavel.cargo ? ' (' + L.responsavel.cargo + ')' : ''}` }));
    const br = d => d.split('-').reverse().join('/');
    const periodo = L.validoDe && L.validade ? `Válida de ${br(L.validoDe)} até ${br(L.validade)}`
        : L.validade ? `Válida até ${br(L.validade)}`
            : L.validoDe ? `Válida a partir de ${br(L.validoDe)}`
                : L.nome ? 'Sem prazo de validade' : '';
    if (periodo) info.push(h('div', { class: 'sub', text: periodo }));
    if (!ok || L.bypass) info.push(h('div', { class: 'sub', text: L.message }));
    return h('div', { class: 'card lic' },
        h('div', { class: 'pv sm' }, icon('shield')),
        h('div', { class: 'txt' }, ...info.length ? info : [h('b', { text: 'Licença do cartório' }), h('div', { class: 'sub', text: L.message })],
            h('div', { class: 'badges' }, h('span', { class: estado[0], text: estado[1] }),
                ...(L.layouts || []).map(id => h('span', { class: 'badge', text: id })))),
        h('button', { class: 'btn sm', onclick: importLicense }, icon('save', 'sm'), 'Importar licença'));
}

async function importLicense() {
    try {
        const r = await call('license.import');
        if (!r) return;
        S.license = r;
        S.layouts = await call('layouts.list');
        S.profiles = await call('profiles.list');
        S.layoutId = null;
        S.layoutDetail = null;
        applyLicenseLock();
        buildNav();
        refreshGerarSelects();
        renderConexoes();
        renderLayouts();
        if (r.valid) go('gerar');
        toast(`Licença de “${r.nome}” importada: ${S.layouts.length} modelo(s) liberado(s).`, 'ok');
    } catch (err) {
        toast(err.message, 'err');
    }
}

async function reloadLayouts() {
    S.layouts = await call('layouts.reload');
    S.layoutDetail = null;
    buildNav();
    refreshGerarSelects();
    renderLayouts();
    toast('Layouts recarregados.', 'ok');
}

async function importLayout() {
    try {
        const l = await call('layouts.import');
        if (!l) return;
        S.layouts = await call('layouts.list');
        S.layoutId = l.id;
        S.layoutDetail = null;
        buildNav();
        refreshGerarSelects();
        renderLayouts();
        toast(`Layout “${l.name}” importado.`, 'ok');
    } catch (err) {
        toast(err.message, 'err');
    }
}

// ============================================================ HISTÓRICO
async function loadHistory() {
    try {
        S.history = await call('history.list');
    } catch {
        S.history = [];
    }
    const root = $('#view-historico');
    if (!S.history.length) {
        root.replaceChildren(h('div', {
            class: 'empty card'
        }, icon('history', 'lg'), h('h2', {
            text: 'Nada por aqui ainda'
        }), h('p', {
            text: 'As execuções aparecem aqui depois de “Gerar JSON”.'
        })));
        return;
    }
    const stBadge = s => h('span', {
        class: 'badge ' + (s === 'ok' ? 'ok' : s === 'cancelled' ? 'warn' : 'err'),
        text: s === 'ok' ? 'ok' : s === 'cancelled' ? 'cancelado' : 'erro'
    });
    root.replaceChildren(h('div', {
        class: 'table-wrap',
        style: 'max-height:none'
    }, h('table', {
        class: 't'
    },
        h('thead', {}, h('tr', {}, ...['Quando', 'Status', 'Layout', 'Conexão', 'Modo', 'Registros', 'Tempo', 'Arquivos'].map(x => h('th', {
            text: x
        })))),
        h('tbody', {}, ...S.history.map(r => h('tr', {},
            h('td', {
                text: fmtDate(r.startedAt)
            }), h('td', {}, stBadge(r.status)), h('td', {
                text: r.layoutName
            }), h('td', {
                text: r.profileName
            }),
            h('td', {
                text: r.mode === 'virtual' ? 'virtual' : 'arquivos'
            }), h('td', {
                class: 'mono',
                text: nf(r.rows)
            }), h('td', {
                class: 'mono',
                text: fmtMs(r.elapsedMs)
            }),
            h('td', {}, r.files && r.files.length ? h('button', {
                class: 'btn sm',
                onclick: () => call('shell.open', {
                    path: r.files[0].path
                }).catch(e => toast(e.message, 'err'))
            }, icon('folder', 'sm'), `${r.files.length}`) : h('span', {
                class: 'muted',
                text: r.message ? r.message.slice(0, 40) : '—'
            }))))))));
}

async function clearHistory() {
    if (!(await askConfirm('Limpar histórico', 'Apaga apenas o registro das execuções (os arquivos gerados são mantidos).', 'Limpar'))) return;
    await call('history.clear');
    loadHistory();
}

// ============================================================ carga inicial
async function loadProfiles() {
    S.profiles = await call('profiles.list');
    buildNav();
    renderConexoes();
    refreshGerarSelects();
}

async function init() {
    if (window.chrome && window.chrome.webview) bridge = realBridge();
    else {
        await new Promise(res => {
            const s = document.createElement('script');
            s.src = 'mock.js';
            s.onload = res;
            document.head.appendChild(s);
        });
        bridge = window.__mockBridge(emit);
    }
    S.info = await call('app.info');
    $('#ver').textContent = S.info.version;
    S.providers = await call('providers.list');
    S.license = await call('license.get');
    const liberado = !!(S.license && S.license.valid);
    S.layouts = liberado ? await call('layouts.list') : [];
    S.profiles = liberado ? await call('profiles.list') : [];
    $('#theme').onchange = e => applyTheme(e.target.value);
    applyTheme(prefs.theme || 'matrix');
    buildNav();
    buildGerar();
    refreshGerarSelects();
    renderConexoes();
    renderLayouts();
    go(prefs.view && VIEWS[prefs.view] ? prefs.view : 'gerar');
    applyLicenseLock();
    if (liberado && G.el.layout.value) refreshFilterColumns();
    if (liberado && S.info.layoutErrors && S.info.layoutErrors.length) toast('Layouts ignorados: ' + S.info.layoutErrors.join(' | '), 'err');
}

document.addEventListener('keydown', e => {
    if (e.ctrlKey && e.key === 'Enter' && S.view === 'gerar' && !G.running && S.license && S.license.valid) doRun();
});

init().catch(err => {
    document.body.prepend(h('pre', {
        style: 'color:#f43f5e;padding:20px',
        text: 'Falha ao iniciar: ' + err.message
    }));
});