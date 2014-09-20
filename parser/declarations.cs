﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// declaration : declaration_specifiers [init_declarator_list]? ;
// [ return: Declaration ]
public class _declaration : PTNode {
    public static int Parse(List<Token> src, int begin, out Declaration declaration) {
        declaration = null;

        DeclarationSpecifiers declaration_specifiers;
        int current = _declaration_specifiers.Parse(src, begin, out declaration_specifiers);
        if (current == -1) {
            return -1;
        }

        int saved = current;
        List<InitDeclarator> init_declarators;
        current = _init_declarator_list.Parse(src, current, out init_declarators);
        if (current == -1) {
            current = saved;
        }

        if (src[current].type != TokenType.OPERATOR) {
            return -1;
        }
        if (((TokenOperator)(src[current])).val != OperatorVal.SEMICOLON) {
            return -1;
        }

        declaration = new Declaration();
        declaration.declaration_specifiers = declaration_specifiers;
        declaration.init_declarators = init_declarators;
        if (declaration_specifiers.IsTypedef()) {
            foreach (InitDeclarator init_declarator in init_declarators) {
                ScopeEnvironment.AddTypedefName(init_declarator.declarator.name);
            }
        }

        current++;
        return current;

    }
}

public class Declaration : ASTNode {
    public DeclarationSpecifiers declaration_specifiers;
    public List<InitDeclarator> init_declarators;
}


// declaration_specifiers : storage_class specifier [declaration_specifiers]?
//                        | type_specifier [declaration_specifiers]?
//                        | type_qualifier [declaration_specifiers]?
// [ note: my solution ]
// declaration_specifiers : [storage_class_specifier | type_specifier | type_qualifier] [declaration_specifiers]?
//
// [ return: DeclarationSpecifiers ]
public class _declaration_specifiers : PTNode {
    public static int Parse(List<Token> src, int begin, out DeclarationSpecifiers node) {
        node = null;
        DeclarationSpecifiers.Type type;
        ASTNode content = null;
        int current;

        StorageClassSpecifier storage_class_specifier;
        if ((current = _storage_class_specifier.Parse(src, begin, out storage_class_specifier)) != -1) {
            // storage_class_specifier successful match
            content = storage_class_specifier;
            type = DeclarationSpecifiers.Type.STORAGE_CLASS_SPECIFIER;
        } else {
            TypeSpecifier type_specifier;
            if ((current = _type_specifier.Parse(src, begin, out type_specifier)) != -1) {
                // type_specifier successful match
                content = type_specifier;
                type = DeclarationSpecifiers.Type.TYPE_SPECIFIER;
            } else {
                TypeQualifier type_qualifier;
                if ((current = _type_qualifier.Parse(src, begin, out type_qualifier)) != -1) {
                    // type_qualifier successful match
                    content = type_qualifier;
                    type = DeclarationSpecifiers.Type.TYPE_QUALIFIER;
                } else {
                    // all fail, return
                    return -1;
                }
            }
        }

        node = new DeclarationSpecifiers(type, content, null);
        // now check whether their is a next

        int saved = current;
        if ((current = _declaration_specifiers.Parse(src, current, out node.next)) != -1) {
            return current;
        } else {
            return saved;
        }

    }
}

public class DeclarationSpecifiers : ASTNode {
    public DeclarationSpecifiers(Type _type, ASTNode _content, DeclarationSpecifiers _next) {
        type = _type;
        content = _content;
        next = _next;
    }
    public enum Type {
        STORAGE_CLASS_SPECIFIER,
        TYPE_SPECIFIER,
        TYPE_QUALIFIER
    };
    public bool IsTypedef() {
        if (type == Type.STORAGE_CLASS_SPECIFIER) {
            if (((StorageClassSpecifier)content).content == KeywordVal.TYPEDEF) {
                return true;
            }
        }
        if (next == null) {
            return false;
        }
        return next.IsTypedef();
    }
    public Type type;
    public ASTNode content;
    public DeclarationSpecifiers next;
}


// init_declarator_list : init_declarator
//                      | init_declarator_list , init_declarator
// [ note: my solution ]
// init_declarator_list : init_declarator [, init_declarator]*
//
// [ return: List<InitDeclarator> ]
// [ if fail, return empty List<InitDeclarator> ]
public class _init_declarator_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<InitDeclarator> init_declarators) {
        init_declarators = new List<InitDeclarator>();

        InitDeclarator init_declarator;
        int current = _init_declarator.Parse(src, begin, out init_declarator);
        if (current == -1) {
            return -1;
        }

        if (src[current].type != TokenType.OPERATOR) {
            init_declarators.Add(init_declarator);
            return current;
        }
        if (((TokenOperator)src[current]).val != OperatorVal.COMMA) {
            init_declarators.Add(init_declarator);
            return current;
        }

        current++;
        int saved = current;
        current = _init_declarator_list.Parse(src, current, out init_declarators);
        init_declarators.Insert(0, init_declarator);
        if (current != -1) {
            return current;
        } else {
            return saved;
        }

    }
}


// init_declarator : declarator [= initializer]?
// [[note: no support for = initializer now]]
// return InitDeclarator / null
public class _init_declarator : PTNode {
    public static int Parse(List<Token> src, int begin, out InitDeclarator init_declarator) {
        init_declarator = null;

        Declarator declarator;
        int current = _declarator.Parse(src, begin, out declarator);
        if (current != -1) {
            init_declarator = new InitDeclarator();
            init_declarator.declarator = declarator;
            return current;
        } else {
            return -1;
        }
    }
}

public class InitDeclarator : ASTNode {
    public Declarator declarator;
}


// storage_class_specifier : auto | register | static | extern | typedef
// [ return: StorageClassSpecifier / null]
// [ note: there can be only one storage class in one declaration ]
public class _storage_class_specifier : PTNode {
    public static int Parse(List<Token> src, int begin, out StorageClassSpecifier node) {
        node = null;

        // make sure the token is a keyword
        if (src[begin].type != TokenType.KEYWORD)
            return -1;

        // check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.AUTO:
        case KeywordVal.REGISTER:
        case KeywordVal.STATIC:
        case KeywordVal.EXTERN:
        case KeywordVal.TYPEDEF:
            node = new StorageClassSpecifier(val);
            return begin + 1;
        default:
            return -1;
        }

    }
}

public class StorageClassSpecifier : ASTNode {
    public StorageClassSpecifier(KeywordVal _content) {
        content = _content;
    }
    public KeywordVal content;
}


// type_specifier : void | char | short | int | long | float | double | signed | unsigned
//                | struct_or_union_specifier | enum_specifier | typedef_name
// [ return: TypeSpecifier / null ]
// [note: the last three need scope environment!]
// [[note: no support for struct, union, enum, typedef]]
// [ note: there can be multiple type specifiers in one declaration ]
// [ note: typedef_name is some previously typedefed symbol ]
public class _type_specifier : PTNode {
    public static int Parse(List<Token> src, int begin, out TypeSpecifier node) {
        node = null;

        // make sure the token is a keyword
        if (src[begin].type != TokenType.KEYWORD)
            return -1;

        // check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.VOID:
        case KeywordVal.CHAR:
        case KeywordVal.SHORT:
        case KeywordVal.INT:
        case KeywordVal.LONG:
        case KeywordVal.FLOAT:
        case KeywordVal.DOUBLE:
        case KeywordVal.SIGNED:
        case KeywordVal.UNSIGNED:
            node = new TypeSpecifier(val);
            return begin + 1;
        default:
            return -1;
        }

    }
}

public class TypeSpecifier : ASTNode {
    public TypeSpecifier(KeywordVal _content) {
        content = _content;
    }
    public KeywordVal content;
}


// type_qualifier : const | volatile
// [ return: TypeQualifier / null]
// [ note: there can be multiple type_qualifiers in one declaration ]
public class _type_qualifier : PTNode {
    public static int Parse(List<Token> src, int begin, out TypeQualifier node) {
        node = null;

        // make sure te token is a keyword
        if (src[begin].type != TokenType.KEYWORD)
            return -1;

        // check the value
        KeywordVal val = ((TokenKeyword)src[begin]).val;
        switch (val) {
        case KeywordVal.CONST:
        case KeywordVal.VOLATILE:
            node = new TypeQualifier(val);
            return begin + 1;
        default:
            return -1;
        }

    }
}

public class TypeQualifier : ASTNode {
    public TypeQualifier(KeywordVal _content) {
        content = _content;
    }
    public KeywordVal content;
}


// declarator : [pointer]? direct_declarator
// [ return: Declarator / null ]
public class _declarator : PTNode {
    public static int Parse(List<Token> src, int begin, out Declarator node) {
        node = null;
        List<PointerInfo> pointer_infos;
        int current = _pointer.Parse(src, begin, out pointer_infos);
        if (current == -1) {
            current = begin;
        }

        current = _direct_declarator.Parse(src, current, out node);
        if (current != -1) {
            node.type_infos.AddRange(pointer_infos);
            return current;
        } else {
            return -1;
        }
    }
}

public interface TypeInfo {
}

public class FunctionInfo : TypeInfo {

}

public class ArrayInfo : TypeInfo {

}

public class PointerInfo : TypeInfo {
    public List<TypeQualifier> type_qualifiers;
}

public class Declarator : ASTNode {
    public Declarator() {
        type_infos = new List<TypeInfo>();
    }
    public List<TypeInfo> type_infos;
    public String name;
}


// pointer : * [type_qualifier_list]? [pointer]?
// [ note: my solution ]
// pointer : < * [type_qualifier_list]? >+
// [ return: List<PointerInfo> ]
// [ if fail, return empty List<PointerInfo> ]
public class _pointer : PTNode {
    public static int Parse(List<Token> src, int begin, out List<PointerInfo> infos) {
        infos = new List<PointerInfo>();
        if (src[begin].type != TokenType.OPERATOR) {
            return -1;
        }
        if (((TokenOperator)src[begin]).val != OperatorVal.MULT) {
            return -1;
        }

        PointerInfo info = new PointerInfo();
        int current = _type_qualifier_list.Parse(src, begin + 1, out info.type_qualifiers);
        if (current == -1) {
            current = begin + 1;
        }

        int saved = current;
        current = _pointer.Parse(src, current, out infos);
        infos.Add(info);
        if (current != -1) {
            return current;
        } else {
            return saved;
        }
    }
}

// parameter_type_list : parameter_list
//                     | parameter_list , ...
// [ note: my solution ]
// parameter_type_list : parameter_list < , ... >?
public class _parameter_type_list : PTNode {
    public static int Parse(List<Token> src, int begin, out ParameterTypeList param_type_list) {
        param_type_list = null;

        List<ParameterDeclaration> param_list;
        int current = _parameter_list.Parse(src, begin, out param_list);
        if (current == -1) {
            return -1;
        }

        param_type_list = new ParameterTypeList(param_list);

        if (Parser.IsCOMMA(src[current])) {
            int saved = current;
            current++;
            if (Parser.IsEllipsis(src, current)) {
                current += 3;
                param_type_list.IsVarArgs = true;
                return current;
            } else {
                current = saved;
            }
        }

        return current;
    }
}

public class ParameterTypeList : ASTNode {
    public ParameterTypeList(List<ParameterDeclaration> _param_list) {
        IsVarArgs = false;
        param_list = _param_list;
    }

    public bool IsVarArgs;
    public List<ParameterDeclaration> param_list;
}


// parameter_list : parameter_declaration
//                | parameter_list, parameter_declaration
// [ note: my solution ]
// parameter_list : parameter_declaration < , parameter_declaration >*
// [ note: it's okay to have a lonely ',', just leave it alone ]
public class _parameter_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<ParameterDeclaration> param_list) {
        ParameterDeclaration decl;
        int current = _parameter_declaration.Parse(src, begin, out decl);
        if (current == -1) {
            param_list = null;
            return -1;
        }

        param_list = new List<ParameterDeclaration>();
        param_list.Add(decl);

        int saved;
        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _parameter_declaration.Parse(src, current, out decl);
                if (current == -1) {
                    return saved;
                }
                param_list.Add(decl);
            } else {
                return current;
            }
        }
    }
}


// type_qualifier_list : [type_qualifier]+
// [ return: List<TypeQualifier> ]
// [ if fail, return empty List<TypeQualifier> ]
public class _type_qualifier_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<TypeQualifier> type_qualifiers) {
        type_qualifiers = new List<TypeQualifier>();

        TypeQualifier type_qualifier;
        int current = _type_qualifier.Parse(src, begin, out type_qualifier);
        if (current == -1) {
            return -1;
        }

        int saved = current;
        current = _type_qualifier_list.Parse(src, current, out type_qualifiers);
        type_qualifiers.Insert(0, type_qualifier);
        if (current != -1) {
            return current;
        } else {
            return saved;
        }

    }
}


// direct_declarator : identifier
//                   | '(' declarator ')'
//                   | direct_declarator '[' [constant_expression]? ']'
//                   | direct_declarator '(' [parameter_type_list]? ')'
//                   | direct_declarator '(' identifier_list ')' /* old style, i'm deleting this */
// [ return: Declarator / null ]
// [ note: left recursion! ]
// { note: simplified! no const_expr, param_type_list, id_list now }
// direct_declarator : < identifier | ( declarator ) > < [ ] | ( ) >*
public class _direct_declarator : PTNode {
    public static int Parse(List<Token> src, int begin, out Declarator node) {
        node = null;
        int current;

        // match id | ( declarator )
        if (src[begin].type == TokenType.IDENTIFIER) {
            // match id
            String name = ((TokenIdentifier)src[begin]).val;
            //if (ScopeEnvironment.IsNewId(name)) {
            // ScopeEnvironment.AddVar(name);
            if (true) {
                node = new Declarator();
                node.name = name;
                current = begin + 1;
            } else {
                // have seen this identifier, not good
                return -1;
            }
        } else if (src[begin].type == TokenType.OPERATOR) {
            // match ( declarator )
            if (((TokenOperator)src[begin]).val != OperatorVal.LPAREN) {
                return -1;
            }
            current = begin + 1;
            current = _direct_declarator.Parse(src, current, out node);
            if (current == -1) {
                return -1;
            }
            if (src[current].type != TokenType.OPERATOR) {
                return -1;
            }
            if (((TokenOperator)src[current]).val != OperatorVal.RPAREN) {
                return -1;
            }
            current++;
        } else {
            return -1;
        }

        if (src[current].type != TokenType.OPERATOR) {
            return current;
        }
        while (src[current].type == TokenType.OPERATOR) {
            OperatorVal op = ((TokenOperator)src[current]).val;
            if (op == OperatorVal.LPAREN) {
                current++;
                op = ((TokenOperator)src[current]).val;
                if (op == OperatorVal.RPAREN) {
                    current++;
                    node.type_infos.Add(new FunctionInfo());
                } else {
                    return -1;
                }
            } else if (op == OperatorVal.LBRACKET) {
                current++;
                op = ((TokenOperator)src[current]).val;
                if (op == OperatorVal.RBRACKET) {
                    current++;
                    node.type_infos.Add(new ArrayInfo());
                } else {
                    return -1;
                }
            } else {
                return current;
            }
        }
        return current;
    }
}


// enum_specifier : enum <identifier>? { enumerator_list }
//                | enum identifier
public class _enum_specifier : PTNode {

    // this parses { enumerator_list }
    private static int ParseEnumList(List<Token> src, int begin, out List<Enumerator> enum_list) {
        enum_list = null;
        if (!Parser.IsLCURL(src[begin])) {
            return -1;
        }
        int current = begin + 1;
        current = _enumerator_list.Parse(src, current, out enum_list);
        if (current == -1) {
            return -1;
        }
        if (!Parser.IsRCURL(src[current])) {
            return -1;
        }
        current++;
        return current;
    }

    public static int Parse(List<Token> src, int begin, out EnumSpecifier enum_spec) {
        enum_spec = null;
        if (src[begin].type != TokenType.KEYWORD) {
            return -1;
        }
        if (((TokenKeyword)src[begin]).val != KeywordVal.ENUM) {
            return -1;
        }

        int current = begin + 1;
        List<Enumerator> enum_list;
        if (src[current].type == TokenType.IDENTIFIER) {
            enum_spec = new EnumSpecifier(((TokenIdentifier)src[current]).val, null);
            current++;
            int saved = current;
            current = ParseEnumList(src, current, out enum_list);
            if (current == -1) {
                return saved;
            }
            enum_spec.enum_list = enum_list;
            return current;

        } else {
            current = ParseEnumList(src, current, out enum_list);
            if (current == -1) {
                return -1;
            }
            enum_spec = new EnumSpecifier("", enum_list);
            return current;

        }
    }
}

public class EnumSpecifier : ASTNode {
    public EnumSpecifier(String _name, List<Enumerator> _enum_list) {
        name = _name;
        enum_list = _enum_list;
    }
    public String name;
    public List<Enumerator> enum_list;
}


// enumerator_list : enumerator
//                 | enumerator_list, enumerator
// [ note: my solution ]
// enumerator_list : enumerator < , enumerator >*
public class _enumerator_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<Enumerator> enum_list) {
        Enumerator enumerator;
        enum_list = new List<Enumerator>();
        int current = _enumerator.Parse(src, begin, out enumerator);
        if (current == -1) {
            return -1;
        }
        enum_list.Add(enumerator);
        int saved;

        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _enumerator.Parse(src, current, out enumerator);
                if (current == -1) {
                    return saved;
                }
                enum_list.Add(enumerator);
            } else {
                return current;
            }
        }
    }
}


// enumerator : enumeration_constant
//            | enumeration_constant = constant_expression
// [ note: my solution ]
// enumerator : enumeration_constant < = constant_expression >?
public class _enumerator : PTNode {
    public static int Parse(List<Token> src, int begin, out Enumerator enumerator) {
        int current = _enumeration_constant.Parse(src, begin, out enumerator);
        if (current == -1) {
            return -1;
        }

        if (Parser.IsAssignment(src[current])) {
            current++;
            Expression init;
            current = _constant_expression.Parse(src, current, out init);
            if (current == -1) {
                return -1;
            }

            enumerator.init = init;
            return current;
        }

        return current;
    }
}

public class Enumerator : ASTNode {
    public Enumerator(String _name, Expression _init) {
        name = _name;
        init = _init;
    }
    public Expression init;
    public String name;
}


// enumeration_constant : identifier
public class _enumeration_constant : PTNode {
    public static int Parse(List<Token> src, int begin, out Enumerator enumerator) {
        if (src[begin].type == TokenType.IDENTIFIER) {
            enumerator = new Enumerator(((TokenIdentifier)src[begin]).val, null);
            return begin + 1;
        }
        enumerator = null;
        return -1;
    }
}


// struct_or_union_specifier : struct_or_union <identifier>? { struct_declaration_list }
//                           | struct_or_union identifier
// [ note: need some treatment ]
public class _struct_or_union_specifier : PTNode {
    public static int ParseDeclarationList(List<Token> src, int begin, out List<StructDecleration> decl_list) {
        decl_list = null;

        if (!Parser.IsLCURL(src[begin])) {
            return -1;
        }
        int current = begin + 1;
        current = _struct_declaration_list.Parse(src, current, out decl_list);
        if (current == -1) {
            return -1;
        }

        if (!Parser.IsRCURL(src[current])) {
            return -1;
        }
        current++;
        return current;

    }

    public static int Parse(List<Token> src, int begin, out StructOrUnionSpecifier spec) {
        spec = null;

        StructOrUnion struct_or_union;
        List<StructDecleration> decl_list;

        int current = _struct_or_union.Parse(src, begin, out struct_or_union);
        if (current == -1) {
            return -1;
        }
        //current++;

        if (src[current].type == TokenType.IDENTIFIER) {
            // named struct or union

            String name = ((TokenIdentifier)src[current]).val;
            if (struct_or_union.is_union) {
                spec = new UnionSpecifier(name, null);
            } else {
                spec = new StructSpecifier(name, null);
            }
            current++;
            int saved = current;

            current = ParseDeclarationList(src, current, out decl_list);
            if (current != -1) {
                spec.decl_list = decl_list;
                return current;
            }

            return current;

        } else {
            // anonymous struct or union

            current = ParseDeclarationList(src, current, out decl_list);
            if (current == -1) {
                return -1;
            }

            spec = new StructSpecifier("", decl_list);
            return current;

        }
    }
}

public class StructOrUnionSpecifier : ASTNode {
    public String name;
    public List<StructDecleration> decl_list;
}

public class StructSpecifier : StructOrUnionSpecifier {
    public StructSpecifier(String _name, List<StructDecleration> _decl_list) {
        name = _name;
        decl_list = _decl_list;
    }
}

public class UnionSpecifier : StructOrUnionSpecifier {
    public UnionSpecifier(String _name, List<StructDecleration> _decl_list) {
        name = _name;
        decl_list = _decl_list;
    }
}


// struct_or_union : struct | union
public class _struct_or_union : PTNode {
    public static int Parse(List<Token> src, int begin, out StructOrUnion struct_or_union) {
        struct_or_union = null;
        if (src[begin].type != TokenType.KEYWORD) {
            return -1;
        }
        switch (((TokenKeyword)src[begin]).val) {
        case KeywordVal.STRUCT:
            struct_or_union = new StructOrUnion(false);
            return begin + 1;
        case KeywordVal.UNION:
            struct_or_union = new StructOrUnion(true);
            return begin + 1;
        default:
            return -1;
        }
    }
}

public class StructOrUnion : ASTNode {
    public StructOrUnion(bool _is_union) {
        is_union = _is_union;
    }
    public bool is_union;
}


// struct_declaration_list : struct_declaration
//                         | struct_declaration_list struct_declaration
// [ note: my solution ]
// struct_declaration_list : <struct_declaration>+
public class _struct_declaration_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<StructDecleration> decl_list) {
        decl_list = new List<StructDecleration>();

        StructDecleration decl;
        int current = _struct_declaration.Parse(src, begin, out decl);
        if (current == -1) {
            return -1;
        }
        decl_list.Add(decl);

        int saved;
        while (true) {
            saved = current;
            current = _struct_declaration.Parse(src, current, out decl);
            if (current == -1) {
                return saved;
            }
            decl_list.Add(decl);
        }
    }
}


// struct_declaration : specifier_qualifier_list struct_declarator_list ;
public class _struct_declaration : PTNode {
    public static int Parse(List<Token> src, int begin, out StructDecleration decl) {
        decl = null;

        DeclarationSpecifiers specs;
        List<Declarator> decl_list;
        int current = _specifier_qualifier_list.Parse(src, begin, out specs);
        if (current == -1) {
            return -1;
        }
        current = _struct_declarator_list.Parse(src, current, out decl_list);
        if (current == -1) {
            return -1;
        }
        if (!Parser.IsSEMICOLON(src[current])) {
            return -1;
        }

        current++;
        decl = new StructDecleration(specs, decl_list);
        return current;
    }
}

public class StructDecleration : ASTNode {
    public StructDecleration(DeclarationSpecifiers _specs, List<Declarator> _decl_list) {
        specs = _specs;
        decl_list = _decl_list;
    }
    public DeclarationSpecifiers specs;
    public List<Declarator> decl_list;
}

// specifier_qualifier_list : type_specifier <specifier_qualifier_list>?
//                          | type_qualifier <specifier_qualifier_list>?
// [ note: my solution ]
// specifier_qualifier_list : < type_specifier | type_qualifier >+
public class _specifier_qualifier_list : PTNode {
    public static int Parse(List<Token> src, int begin, out DeclarationSpecifiers node) {
        node = null;
        DeclarationSpecifiers.Type type;
        ASTNode content = null;
        int current;


        TypeSpecifier type_specifier;
        if ((current = _type_specifier.Parse(src, begin, out type_specifier)) != -1) {
            // type_specifier successful match
            content = type_specifier;
            type = DeclarationSpecifiers.Type.TYPE_SPECIFIER;
        } else {
            TypeQualifier type_qualifier;
            if ((current = _type_qualifier.Parse(src, begin, out type_qualifier)) != -1) {
                // type_qualifier successful match
                content = type_qualifier;
                type = DeclarationSpecifiers.Type.TYPE_QUALIFIER;
            } else {
                // all fail, return
                return -1;
            }
        }


        node = new DeclarationSpecifiers(type, content, null);
        // now check whether their is a next

        int saved = current;
        if ((current = _specifier_qualifier_list.Parse(src, current, out node.next)) != -1) {
            return current;
        } else {
            return saved;
        }

    }
}


// struct_declarator_list : struct_declarator
//                        | struct_declarator_list , struct_declarator
// [ note: my solution ]
// struct_declarator_list : struct_declarator < , struct_declarator >*
public class _struct_declarator_list : PTNode {
    public static int Parse(List<Token> src, int begin, out List<Declarator> decl_list) {
        Declarator decl;
        decl_list = new List<Declarator>();
        int current = _struct_declarator.Parse(src, begin, out decl);
        if (current == -1) {
            return -1;
        }
        decl_list.Add(decl);
        int saved;

        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _struct_declarator.Parse(src, current, out decl);
                if (current == -1) {
                    return saved;
                }
                decl_list.Add(decl);
            } else {
                return current;
            }
        }
    }
}


// struct_declarator : declarator
//                   | type_specifier <declarator>? : constant_expression
// [ note: the second is for bit-field ]
// [ note: i'm not supporting bit-field ]
public class _struct_declarator : PTNode {
    public static int Parse(List<Token> src, int begin, out Declarator decl) {
        return _declarator.Parse(src, begin, out decl);
    }
}

// parameter_declaration : declaration_specifiers declarator
//                       | declaration_specifiers <abstract_declarator>?
public class _parameter_declaration : PTNode {
    public static int Parse(List<Token> src, int begin, out ParameterDeclaration decl) {
        decl = null;
        DeclarationSpecifiers specs;
        int current = _declaration_specifiers.Parse(src, begin, out specs);
        if (current == -1) {
            return -1;
        }
        int saved = current;

        Declarator declarator;
        current = _declarator.Parse(src, current, out declarator);
        if (current != -1) {
            decl = new ParameterDeclaration(specs, declarator);
            return current;
        }

        current = saved;
        AbstractDeclarator abstract_declarator;
        current = _abstract_declarator.Parse(src, current, out abstract_declarator);
        if (current != -1) {
            decl = new ParameterDeclaration(specs, abstract_declarator);
            return current;
        }

        decl = new ParameterDeclaration(specs);
        return saved;

    }
}

public class ParameterDeclaration : ASTNode {
    public ParameterDeclaration(DeclarationSpecifiers _specs) {
        specs = _specs;
        decl = null;
        abstract_decl = null;
    }

    public ParameterDeclaration(DeclarationSpecifiers _specs, Declarator _decl) {
        specs = _specs;
        decl = _decl;
        abstract_decl = null;
    }

    public ParameterDeclaration(DeclarationSpecifiers _specs, AbstractDeclarator _decl) {
        specs = _specs;
        decl = null;
        abstract_decl = _decl;
    }

    public DeclarationSpecifiers specs;
    public Declarator decl;
    public AbstractDeclarator abstract_decl;
}


// identifier_list : /* old style, i'm deleting this */


// abstract_declarator : pointer
//                     | <pointer>? direct_abstract_declarator
// [ note: this is for anonymous declarator ]
// [ note: there couldn't be any typename in an abstract_declarator ]
public class _abstract_declarator : PTNode {
    public static int Parse(List<Token> src, int begin, out AbstractDeclarator decl) {
        List<PointerInfo> infos;
        int current = _pointer.Parse(src, begin, out infos);
        if (current == -1) {
            return _direct_abstract_declarator.Parse(src, begin, out decl);
        }

        int saved = current;
        current = _direct_abstract_declarator.Parse(src, current, out decl);
        if (current != -1) {
            decl.type_infos.AddRange(infos);
            return current;
        }

        decl = new AbstractDeclarator();
        decl.type_infos.AddRange(infos);
        return saved;

    }
}

public class AbstractDeclarator : ASTNode {
    public AbstractDeclarator() {
        type_infos = new List<TypeInfo>();
    }
    public List<TypeInfo> type_infos;
}

// direct_abstract_declarator : ( abstract_declarator )
//                            | <direct_abstract_declarator>? [ <constant_expression>? ]
//                            | <direct_abstract_declarator>? ( <parameter_type_list>? )
// [ note: my solution ]
// direct_abstract_declarator : ( abstract_declarator ) < < [ <constant_expression>? ] > | < ( <parameter_type_list>? ) > >*
//                            | < < [ <constant_expression>? ] > | < ( <parameter_type_list>? ) > >+
// [ note: parameter_type_list and abstract_declarator are distinguishable ]
// [ note: first let me ignore parameter_type_list and constant_expression ]
public class _direct_abstract_declarator : PTNode {
    private static int ParseInfo(List<Token> src, int begin, out TypeInfo info) {
        info = null;
        int current;
        if (Parser.IsLPAREN(src[begin])) {
            current = begin + 1;
            if (Parser.IsRPAREN(src[current])) {
                info = new FunctionInfo();
                current++;
                return current;
            }
        }
        if (Parser.IsLBRACKET(src[begin])) {
            current = begin + 1;
            if (Parser.IsRBRACKET(src[current])) {
                info = new ArrayInfo();
                current++;
                return current;
            }
        }
        return -1;
    }

    public static int Parse(List<Token> src, int begin, out AbstractDeclarator decl) {
        TypeInfo info;
        List<TypeInfo> type_infos;
        int current;
        int saved;

        if (Parser.IsLPAREN(src[begin])) {
            current = begin + 1;
            current = _abstract_declarator.Parse(src, current, out decl);
            if (current != -1) {
                type_infos = decl.type_infos;
                if (!Parser.IsRPAREN(src[current])) {
                    decl = null;
                    return -1;
                }
                current++;

                while (true) {
                    saved = current;
                    current = ParseInfo(src, current, out info);
                    if (current == -1) {
                        decl = new AbstractDeclarator();
                        decl.type_infos = type_infos;
                        return saved;
                    }
                    type_infos.Add(info);
                }
            }
        }

        // if not start with abstract declarator
        type_infos = new List<TypeInfo>();
        current = ParseInfo(src, begin, out info);
        if (current == -1) {
            decl = null;
            return -1;
        }
        type_infos.Add(info);
        while (true) {
            saved = current;
            current = ParseInfo(src, current, out info);
            if (current == -1) {
                decl = new AbstractDeclarator();
                decl.type_infos = type_infos;
                return saved;
            }
            type_infos.Add(info);
        }

    }
}

// initializer : assignment_expression
//             | { initializer_list }
//             | { initializer_list , }
public class _initializer : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        if (!Parser.IsLCURL(src[begin])) {
            return _assignment_expression.Parse(src, begin, out node);
        }

        int current = begin + 1;
        current = _initializer_list.Parse(src, current, out node);
        if (current == -1) {
            return -1;
        }

        if (Parser.IsRCURL(src[current])) {
            current++;
            return current;
        }

        if (!Parser.IsCOMMA(src[current])) {
            return -1;
        }

        current++;
        if (!Parser.IsRCURL(src[current])) {
            return -1;
        }

        current++;
        return current;
    }
}


// initializer_list : initializer
//                  | initializer_list , initializer
// [ note: my solution ]
// initializer_list : initializer < , initializer >*
// [ leave single ',' alone ]
public class _initializer_list : PTNode {
    public static int Parse(List<Token> src, int begin, out Expression node) {
        node = null;
        Expression expr;
        List<Expression> exprs = new List<Expression>();
        int current = _initializer.Parse(src, begin, out expr);
        if (current == -1) {
            return -1;
        }
        exprs.Add(expr);
        int saved;

        while (true) {
            if (Parser.IsCOMMA(src[current])) {
                saved = current;
                current++;
                current = _initializer.Parse(src, current, out expr);
                if (current == -1) {
                    node = new InitializerList(exprs);
                    return saved;
                }
                exprs.Add(expr);
            } else {
                node = new InitializerList(exprs);
                return current;
            }
        }
    }
}

public class InitializerList : Expression {
    public InitializerList(List<Expression> _exprs) {
        exprs = _exprs;
    }
    public List<Expression> exprs;
}


// type_name : specifier_qualifier_list <abstract_declarator>?
public class _type_name : PTNode {
    public static int Parse(List<Token> src, int begin, out TypeName type_name) {
        type_name = null;
        DeclarationSpecifiers specs;
        int current = _specifier_qualifier_list.Parse(src, begin, out specs);
        if (current == -1) {
            return -1;
        }

        int saved = current;
        AbstractDeclarator decl;
        current = _abstract_declarator.Parse(src, current, out decl);
        if (current == -1) {
            type_name = new TypeName(specs, null);
            return saved;
        }
        type_name = new TypeName(specs, decl);
        return current;
    }
}

public class TypeName : ASTNode {
    public TypeName(DeclarationSpecifiers _specs, AbstractDeclarator _decl) {
        specs = _specs;
        decl = _decl;
    }
    public DeclarationSpecifiers specs;
    public AbstractDeclarator decl;
}


// typedef_name : identifier
// [ note: must be something already defined, so this needs environment ]
public class _typedef_name : PTNode {
    public static int Parse(List<Token> src, int begin, out String name) {
        name = null;
        if (src[begin].type != TokenType.IDENTIFIER) {
            return -1;
        }
        if (!ScopeEnvironment.HasTypedefName(((TokenIdentifier)src[begin]).val)) {
            return -1;
        }

        name = ((TokenIdentifier)src[begin]).val;
        return begin + 1;
    }
}

