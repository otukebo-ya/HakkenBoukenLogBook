using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ColorBath
{
    public class GeminiPrompts
    {
        // キャラ付けと、やっていることをGeminiAPIに教えるためのプロンプト
        public static string GetBackBonePrompt(string theme)
        {
            string prompt = 
            $@"
            ＜あなたの設定＞
            あなたは高飛車なお嬢様です。
            私に対してフレンドリーな口調で接します。
            また、細かいところに気を配り人をほめることが得意です。
            顔文字は使わないようにしましょう。
            ＜私の設定＞
            私はカラーバスを行っています。
            日ごとにテーマを設定し、それに該当するモノをたくさん探すことで、身の回りのモノへの意識を高まり、発想力の強化につなげます。
            ";

            return prompt;
        }

        public static string GetReviewPrompt(History history)
        {
            // 先日の履歴から、発見と相槌を取り出し
            Discovery[] discoveries = history.Discoveries;
            string discoveriesText = "";
            for (int d = 1; d <= discoveries.Length; d++)
            {
                discoveriesText += $@"<発見{d}個目>\n"
                                + $@"　・私の発見：{discoveries[d - 1].Memo}\n"
                                + $@"　・あなたの相槌：{discoveries[d - 1].Aizuchi}\n";
            }

            // 背景とタスクを合わせる
            string prompt = 
            $@"
            ＜タスク＞
            今から、私がテーマに沿って発見したものとそれに対するあなたの相槌のログを教えます。
            私の発見について、簡単に総括を行ったうえで、
            レビューを行ってください。
            内容はポジティブだといいな。
            文字数は200字以内とします。
            ＜ログ＞
            {discoveriesText}
            ";

            return prompt;
        }

        public static string GetAizuchiPrompt(string backbone, string theme, string input)
        {
            string prompt =
            $@"
            {backbone}\n
            ＜タスク＞
            今から、私の発見したものをテキスト形式であなたに渡します。
            あなたはそれを確認し、私のその発見に対する相槌を行ってください。
            文字数は100文字以内とします。
            本日のカラーバスのテーマは{theme}なので、そのことを踏まえた相槌を行ってください。
            カラーバスを行うことは当たり前なので、”カラーバス頑張っているのね”のような、カラーバスの取り組んでいることについての言及を行わないでください。
            文字数がもったいないので。
            ＜入力＞
            {input}
            ";
            return prompt;
        }

        public static string GetThemeDecidePrompt(string themes)
        {
            // ランダム要素を入れて、固定化を防ぐ
            string randomizer = Guid.NewGuid().ToString();
            string prompt = 
            $@"
            ## タスク
            私はカラーバスを行っているよ。
            これは、日によって異なるさまざまなテーマを設定し、該当するモノをたくさん探すことで、身の回りのモノへの意識を高まり、発想力の強化につながるんだ。
            ここ数日間のテーマはthemesに記述するかも。でも、ここ数日のテーマはない場合もあるよ！
            過去テーマと被らないほうがいいね。
            本日探すべきテーマを単語としてランダムに選んでほしいな。
            ここで、カラーバスは見た目の話だから、見た目から感じ取れるテーマである必要があるから、
            注意してほしいな。
            色、形、質感、擬音、感覚、そのほかいろんなジャンルがあるよね。
            あなたには自由に様々なジャンルからユニークなものを選んでほしいね。
            テーマは抽象的だと、あてはまるものが多くなってカラーバスが楽しくなるな～。
            
            ## 応答の形式
            {{
                [""theme"": ""提案するテーマ""]
            }}

            ## themas（過去数日間のテーマ）ないこともある
            {themes}

            ## randomizer（発想にもちいる乱数）
            {randomizer}
            ";

            return prompt;
        }
    }
}